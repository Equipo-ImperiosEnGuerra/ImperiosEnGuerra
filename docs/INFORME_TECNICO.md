# Informe Técnico — Imperios en Guerra

## 1. Descripción general

**Imperios en Guerra** es un RTS académico de escritorio desarrollado con **C# y Unity**, inspirado en Age of Empires. La modalidad final evaluada es **1 jugador Humano contra 3 facciones controladas por IA**: Morada, Verde y Amarilla.

La guía original contemplaba dos jugadores conectados entre dos instancias. Posteriormente, según la aclaración recibida por el equipo, ese punto dejó de ser obligatorio para la entrega. El proyecto conserva **WebSocket + JSON** como extensión técnica opcional y utiliza una API REST local como puente entre Unity y el núcleo lógico.

El proyecto demuestra principalmente:

- Programación Orientada a Objetos;
- arquitectura MVC;
- concurrencia real con `Task`/ThreadPool;
- sincronización de datos compartidos;
- cancelación cooperativa;
- manejo de archivos con `System.IO`;
- networking opcional;
- validaciones y excepciones;
- pruebas automatizadas;
- integración con Unity como Vista.

## 2. Arquitectura MVC

### 2.1 Modelo

El Modelo se encuentra principalmente en:

```text
src/ImperiosEnGuerra.Modelo/
```

Contiene estado y reglas del juego sin depender de Unity. Entre sus clases principales se encuentran:

- `Partida`;
- `Jugador`;
- `Mapa`;
- `Casilla`;
- `Coordenada`;
- `Recurso`;
- `RecursosJugador`;
- `Unidad`;
- `Aldeano`;
- `Soldado`;
- `Guerrero`;
- `Lancero`;
- `Arquero`;
- `Monje`;
- `Edificio`;
- `CentroUrbano`.

El proyecto del Modelo usa `netstandard2.1` y no referencia `UnityEngine`.

Responsabilidades del Modelo:

- reglas de recursos;
- costos;
- estados de unidades;
- posiciones lógicas;
- vida y daño;
- validación de acciones;
- condición de victoria;
- ocupación del mapa;
- pathfinding;
- reglas de aliados/enemigos;
- decisiones lógicas de IA.

### 2.2 Vista

La Vista se encuentra principalmente en:

```text
Assets/Scripts/Vistas/
```

Clases relevantes:

- `VistaPartida`;
- `VistaHud`;
- `VistaMenuInicial`;
- `VistaMenuPausa`;
- `EntidadSeleccionableVista`;
- `AnimacionRecursoRecoleccion`.

Unity se utiliza para sprites, escena, HUD, cámara, selección, colliders/raycasts, animaciones, menús, feedback de vida crítica y representación gráfica del estado recibido desde la API.

La Vista no contiene las reglas económicas o de combate principales.

### 2.3 Controladores

Los controladores se encuentran en:

```text
Assets/Scripts/Controladores/
```

Entre ellos:

- `ControladorSeleccion`;
- `ControladorAcciones`;
- `ControladorMenuInicial`;
- `ControladorConexionApi`.

Flujo general:

```text
Jugador
  ↓
Unity / Controlador
  ↓
API REST
  ↓
Servicios
  ↓
Modelo C#
  ↓
resultado / snapshot
  ↓
Controlador Unity
  ↓
Vista
```

## 3. Programación Orientada a Objetos

### Herencia

```text
Unidad
├── Aldeano
├── Soldado
│   ├── Guerrero
│   ├── Lancero
│   └── Arquero
└── Monje

Edificio
└── CentroUrbano
```

### Encapsulamiento

El estado interno se protege mediante propiedades y métodos que aplican reglas. Ejemplos:

- `Recurso.Extraer(...)`;
- `RecursosJugador.IntentarGastar(...)`;
- `Unidad.IntentarIniciarOrden(...)`;
- `Partida.IntentarFinalizar(...)`;
- `Edificio.RecibirDanio(...)`.

### Composición

`Partida` contiene jugadores; cada `Jugador` contiene unidades, edificios, recursos económicos y referencia a su mapa.

## 4. Concurrencia

La concurrencia es real y no depende exclusivamente de Coroutines.

El componente base es:

```text
GestorProcesosConcurrentes
```

Utiliza `Task.Run`, por lo que los trabajos se ejecutan mediante hilos del ThreadPool.

Mantiene:

- procesos activos;
- `CancellationTokenSource`;
- resultados concurrentes;
- identificadores de proceso;
- identificador del hilo que ejecutó el trabajo.

### Operaciones concurrentes principales

`ServicioAccionesConcurrentes` ejecuta:

- movimiento;
- recolección;
- construcción;
- entrenamiento;
- ataque;
- curación;
- movimientos idle de IA.

Además existen tareas independientes para:

- ciclo de decisiones de las IAs;
- reacciones automáticas;
- regeneración de recursos;
- monitor de sesión;
- listener WebSocket opcional.

## 5. Sincronización y race conditions

### 5.1 Dos procesos gastando los mismos recursos

Dos workers podrían comprobar simultáneamente que existe saldo suficiente y descontar el mismo dinero dos veces.

`RecursosJugador` usa `lock` y realiza validación y descuento dentro de una misma sección crítica.

### 5.2 Estado global de la partida

`EstadoPartidaService` centraliza operaciones sobre la partida activa y protege el acceso con sincronización.

### 5.3 Procesos concurrentes por unidad

`ServicioAccionesConcurrentes` utiliza `ConcurrentDictionary` para asociar unidades con procesos activos y evitar órdenes incompatibles sobre la misma unidad.

### 5.4 Resultados de workers

`GestorProcesosConcurrentes` utiliza:

- `ConcurrentDictionary`;
- `ConcurrentQueue`;
- `Interlocked`.

### 5.5 Networking

`ServicioRedPartida` mantiene conexiones concurrentes y usa `SemaphoreSlim` para serializar envíos por WebSocket.

## 6. Cancelación

Los procesos utilizan `CancellationToken`.

Casos:

- cancelar una orden;
- cancelar procesos al finalizar la partida;
- detener servicios al salir;
- pausar/reanudar servicios de sesión;
- terminar un worker cuando su unidad deja de ser válida.

## 7. Unity Main Thread

Los workers no modifican directamente objetos de Unity.

```text
Task / worker
   ↓
Modelo C#
   ↓
EstadoPartidaService
   ↓
API / respuesta
   ↓
Unity recibe snapshot
   ↓
VistaPartida / VistaHud en Main Thread
```

Unity utiliza Coroutines para peticiones HTTP y actualización visual, pero no sustituyen los workers de C# exigidos para la concurrencia del gameplay.

## 8. Movimiento

El movimiento lógico utiliza coordenadas de cuadrícula, pasos ortogonales, pathfinding A*, validación de límites y ocupación.

El movimiento normal es concurrente y progresivo.

Los Aldeanos humanos permanecen quietos cuando no tienen tarea para que el HUD pueda detectar inactividad. Los Aldeanos IA pueden realizar un paseo ambiental corto de una casilla.

## 9. Recolección

Ciclo:

```text
seleccionar recurso
→ aproximarse
→ extraer
→ cargar
→ volver al Centro Urbano
→ depositar
→ repetir
```

Los nodos agotados pueden regenerarse mediante `ServicioRegeneracionRecursos`.

## 10. Construcción

Incluye costo, validación de ubicación, reserva, progreso, tiempo de construcción, cancelación y finalización lógica.

## 11. Entrenamiento

Incluye costos, cola, progreso, tiempo y spawn seguro.

## 12. Combate y curación

Incluye vida, daño, alcance, intervalo, aproximación automática, eliminación lógica y condición de victoria.

El Monje no realiza daño ofensivo. Cura aliados.

Cuando un Soldado IA recibe un impacto humano, puede reaccionar esa misma unidad atacada como frente defensivo adicional.

## 13. IA

La partida final contiene tres IAs:

- Morada: Arquero → Guerrero → Lancero → Monje;
- Verde: Lancero → Guerrero → Arquero → Monje;
- Amarilla: Guerrero → Arquero → Lancero → Monje.

Reglas relevantes:

- máximo un proceso principal de recolección por facción;
- máximo un frente ofensivo normal por facción;
- una defensa reactiva adicional cuando corresponde;
- paseo idle únicamente para Aldeanos IA;
- economía y actividad militar pueden coexistir.

## 14. Condición de victoria

La regla final adoptada es:

```text
sin Centros Urbanos
Y
sin unidades militares
= facción eliminada
```

El Humano gana cuando las tres IAs quedan eliminadas.

Al finalizar:

- se registra ganador;
- se rechazan nuevas órdenes;
- se cancelan workers;
- se detiene IA;
- Unity bloquea interacción;
- se muestra victoria/derrota;
- se genera `resultado_final.txt`.

## 15. System.IO

La responsabilidad está centralizada en `ServicioArchivos`.

### configuracion.txt

Guarda configuración inicial: jugadores, mapa, saldos, edificios, unidades y recursos físicos.

### log_partida.txt

Registra eventos de la partida.

### resultado_final.txt

Guarda:

- estado final;
- regla de victoria;
- ganador;
- perdedores;
- motivo;
- dimensiones del mapa;
- recursos físicos restantes;
- saldos finales;
- edificios;
- unidades;
- vida;
- obras en curso.

## 16. Manejo de excepciones

Se utilizan validaciones y excepciones como:

- `ArgumentNullException`;
- `ArgumentException`;
- `ArgumentOutOfRangeException`;
- `InvalidOperationException`;
- `IOException`;
- `UnauthorizedAccessException`;
- `OperationCanceledException`.

## 17. Networking opcional

El proyecto conserva un endpoint WebSocket:

```text
/ws/partida
```

Mensajes JSON:

- `MOVER`;
- `RECOLECTAR`;
- `CONSTRUIR`;
- `ENTRENAR`;
- `ATACAR`;
- `CURAR`.

Unity utiliza REST en la modalidad final; WebSocket permanece como demostración técnica adicional.

## 18. Pruebas

La suite .NET cubre Modelo, economía, recolección, movimiento, A*, construcción, entrenamiento, sincronización, ataque, victoria, IA, networking, archivos, cancelación y regeneración.

Última ejecución validada:

```text
Total: 390
Correctas: 390
Errores: 0
Omitidas: 0
```

Las advertencias de acceso denegado a `log_partida.txt` corresponden a una prueba intencional de manejo de IO.

El build Windows standalone fue generado y ejecutado correctamente. El launcher inicia la API, abre el juego y detiene la API cuando el juego se cierra.

## 19. Git

Flujo utilizado:

```text
Issue
→ Branch
→ Desarrollo
→ Pruebas
→ Commit
→ Pull Request
→ Integración
→ main
```

## 20. Conclusión técnica

El proyecto separa el cerebro del juego de su representación gráfica. Las reglas residen en C# y los procesos largos se ejecutan con concurrencia real, mientras Unity se mantiene principalmente como Vista e interfaz.

La solución demuestra POO, MVC, multithreading con Task/ThreadPool, sincronización, cancelación, colecciones concurrentes, System.IO, manejo de excepciones, networking opcional, pruebas automatizadas e integración gráfica con Unity.
