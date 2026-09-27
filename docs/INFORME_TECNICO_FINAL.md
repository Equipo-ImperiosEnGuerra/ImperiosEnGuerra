# Informe técnico final — Imperios en Guerra

## 1. Descripción general

**Imperios en Guerra** es un RTS académico de escritorio desarrollado con **C# y Unity**, inspirado en Age of Empires. La modalidad final evaluada es **1 jugador Humano contra 3 facciones controladas por IA**: Morada, Verde y Amarilla.

La guía original contemplaba dos jugadores conectados entre dos instancias. Posteriormente, según la aclaración recibida por el equipo, ese punto dejó de ser obligatorio para la entrega. El proyecto conserva **WebSocket + JSON** como extensión técnica opcional y utiliza una API REST local como puente entre Unity y el núcleo lógico.

El proyecto busca demostrar principalmente:

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
- `CentroUrbano`;
- operaciones y planificadores de movimiento, recolección, construcción, entrenamiento, ataque y curación.

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
- reglas de alianzas/enemigos;
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

Unity se utiliza para:

- sprites;
- escena;
- HUD;
- cámara;
- interacción visual;
- selección;
- colliders/raycasts;
- animaciones;
- menús;
- feedback de vida crítica;
- representación gráfica del estado recibido desde la API.

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

El flujo general es:

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

Esto permite mantener a Unity principalmente como capa visual y de interacción.

## 3. Programación Orientada a Objetos

El proyecto utiliza:

### Herencia

Ejemplos:

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

El estado interno se protege mediante propiedades de solo lectura y métodos que aplican reglas.

Ejemplos:

- `Recurso.Extraer(...)`;
- `RecursosJugador.IntentarGastar(...)`;
- `Unidad.IntentarIniciarOrden(...)`;
- `Partida.IntentarFinalizar(...)`;
- `Edificio.RecibirDanio(...)`.

### Composición

`Partida` contiene jugadores; cada `Jugador` contiene unidades, edificios, recursos económicos y referencia a su mapa.

### Responsabilidad única

Se separan responsabilidades en operaciones y servicios especializados, evitando una única clase que concentre toda la lógica.

## 4. Concurrencia

La concurrencia es real y no depende exclusivamente de Coroutines.

El componente base es:

```text
GestorProcesosConcurrentes
```

Este gestor utiliza `Task.Run`, por lo que los trabajos se ejecutan mediante hilos del ThreadPool.

Mantiene:

- procesos activos;
- `CancellationTokenSource`;
- resultados concurrentes;
- identificadores de proceso;
- identificador del hilo que ejecutó el trabajo.

### Operaciones concurrentes principales

`ServicioAccionesConcurrentes` ejecuta de forma concurrente:

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

### 5.1 Riesgo: dos procesos gastando los mismos recursos

Dos workers podrían comprobar simultáneamente que existe saldo suficiente y descontar el mismo dinero dos veces.

Solución:

`RecursosJugador` utiliza `lock` y realiza la validación y el descuento dentro de una única sección crítica.

### 5.2 Riesgo: dos workers modificando el estado global

El servicio `EstadoPartidaService` centraliza operaciones sobre la partida y protege su acceso mediante un objeto de sincronización y `lock`.

### 5.3 Riesgo: procesos concurrentes por unidad

`ServicioAccionesConcurrentes` utiliza `ConcurrentDictionary` para asociar unidades con procesos activos y evitar órdenes incompatibles simultáneas.

### 5.4 Resultados de workers

`GestorProcesosConcurrentes` utiliza:

- `ConcurrentDictionary`;
- `ConcurrentQueue`;
- `Interlocked`.

Esto permite publicar resultados desde workers sin depender de colecciones no seguras.

### 5.5 Networking

`ServicioRedPartida` utiliza un `ConcurrentDictionary` para las conexiones y un `SemaphoreSlim` por conexión para impedir dos envíos simultáneos sobre el mismo WebSocket.

## 6. Cancelación

Los procesos utilizan `CancellationToken`.

Ejemplos de cancelación:

- cancelar una orden;
- cancelar procesos al finalizar la partida;
- detener servicios al salir;
- detener IA/reacciones/regeneración durante una pausa;
- cerrar un worker si su unidad deja de ser válida.

La cancelación es cooperativa: el worker consulta el token en puntos seguros y termina sin dejar el Modelo en un estado intermedio inválido.

## 7. Unity Main Thread

Los hilos secundarios no modifican directamente objetos de Unity.

El patrón utilizado es:

```text
Task / worker
   ↓
Modelo C#
   ↓
EstadoPartidaService
   ↓
API / respuesta
   ↓
Unity recibe snapshot en Main Thread
   ↓
VistaPartida / VistaHud
```

Unity utiliza Coroutines para peticiones HTTP y actualización visual, pero esas Coroutines no sustituyen los workers de C# exigidos para la concurrencia del gameplay.

## 8. Movimiento

El movimiento lógico utiliza:

- coordenadas de cuadrícula;
- pasos ortogonales;
- pathfinding A*;
- validación de límites;
- ocupación;
- bloqueo por recursos/edificios/enemigos.

El movimiento normal es concurrente y progresivo.

Los Aldeanos humanos permanecen quietos cuando no tienen tarea para que el HUD pueda detectar y notificar inactividad. Los Aldeanos IA pueden realizar un paseo ambiental corto de una casilla, de baja prioridad.

## 9. Recolección

La recolección sigue el ciclo:

```text
seleccionar recurso
→ aproximarse
→ extraer
→ cargar
→ volver al Centro Urbano
→ depositar
→ repetir mientras sea posible
```

La extracción y los saldos están sincronizados para evitar race conditions.

Los nodos físicos agotados pueden regenerarse mediante `ServicioRegeneracionRecursos`, que ejecuta su ciclo en segundo plano y solicita al Modelo una nueva ubicación válida.

## 10. Construcción

La construcción implementa:

- costo;
- validación de ubicación;
- reserva;
- progreso;
- tiempo de construcción;
- cancelación;
- reembolso cuando corresponde;
- finalización lógica antes de que Unity represente el edificio terminado.

## 11. Entrenamiento

El entrenamiento implementa:

- costos;
- cola;
- progreso;
- tiempo;
- spawn seguro;
- búsqueda de casilla disponible.

Las unidades entrenadas se añaden primero al Modelo y posteriormente Unity las representa desde el snapshot.

## 12. Combate y curación

Las unidades militares poseen estadísticas centralizadas.

El combate incluye:

- vida;
- daño;
- alcance;
- intervalo;
- aproximación automática;
- unidades y edificios como objetivos;
- eliminación lógica;
- liberación de casillas;
- actualización visual;
- condición de victoria.

El Monje no realiza daño ofensivo. Funciona como unidad de apoyo y cura aliados.

Cuando una unidad militar de IA recibe un impacto del humano, puede iniciar una defensa reactiva adicional con **esa misma unidad atacada**. Otras unidades cercanas no se unen automáticamente a ese frente.

## 13. IA

La partida final contiene tres IAs.

Cada facción mantiene economía y actividad militar.

Composición inicial prioritaria:

- Morada: Arquero → Guerrero → Lancero → Monje;
- Verde: Lancero → Guerrero → Arquero → Monje;
- Amarilla: Guerrero → Arquero → Lancero → Monje.

Reglas de control relevantes:

- máximo un proceso de recolección principal por facción;
- máximo un frente ofensivo normal por facción;
- una defensa reactiva adicional cuando corresponde;
- paseo idle de Aldeanos únicamente para IA;
- decisiones económicas y militares pueden coexistir.

## 14. Condición de victoria

La formulación final adoptada es:

```text
sin Centros Urbanos
Y
sin unidades militares
= facción eliminada
```

El humano gana cuando las tres facciones IA cumplen la condición de eliminación.

El humano pierde cuando queda sin Centros Urbanos y sin unidades militares.

Al finalizar:

- se registra el ganador;
- se rechazan nuevas órdenes;
- se cancelan workers;
- se detiene la IA;
- Unity bloquea la interacción;
- se muestra pantalla de victoria/derrota;
- se genera `resultado_final.txt`.

## 15. System.IO

La responsabilidad está centralizada en:

```text
ServicioArchivos
```

Archivos:

### configuracion.txt

Guarda la configuración inicial:

- jugadores;
- tipos;
- dimensiones;
- saldos;
- edificios;
- unidades;
- recursos físicos.

### log_partida.txt

Registra eventos de la partida con un formato estructurado de acción, resultado y mensaje.

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
- edificios restantes y coordenadas;
- unidades restantes y coordenadas;
- vida de entidades;
- obras en curso.

## 16. Manejo de excepciones

Se utilizan validaciones y excepciones para impedir estados inválidos.

Ejemplos:

- `ArgumentNullException`;
- `ArgumentException`;
- `ArgumentOutOfRangeException`;
- `InvalidOperationException`;
- `IOException`;
- `UnauthorizedAccessException`;
- `OperationCanceledException`.

Los errores de archivos y red se controlan para evitar que un fallo secundario destruya el estado lógico de la partida.

## 17. Networking opcional

El proyecto conserva un servidor WebSocket en:

```text
/ws/partida
```

Los mensajes son JSON y soportan, entre otros:

- `MOVER`;
- `RECOLECTAR`;
- `CONSTRUIR`;
- `ENTRENAR`;
- `ATACAR`;
- `CURAR`.

`ServicioRedPartida` escucha clientes de manera asíncrona y `DespachadorMensajesRed` traduce los mensajes a las mismas operaciones concurrentes del gameplay.

Unity utiliza REST en la modalidad final, por lo que el WebSocket se conserva como demostración técnica adicional.

## 18. Pruebas

La suite .NET contiene pruebas de:

- Modelo;
- economía;
- recolección;
- movimiento;
- A*;
- construcción;
- entrenamiento;
- operaciones simultáneas;
- sincronización;
- ataque;
- victoria;
- IA;
- defensa reactiva;
- networking;
- archivos;
- cancelación;
- regeneración de recursos.

Última ejecución validada sobre `main`:

```text
Total: 390
Correctas: 390
Errores: 0
Omitidas: 0
```

Las advertencias de acceso denegado a `log_partida.txt` son intencionales y corresponden a una prueba de manejo de errores de IO.

Unity también contiene pruebas EditMode para elementos visuales y controladores.

## 19. Git

El proyecto fue trabajado con ramas y Pull Requests.

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

Los cambios de estabilización y documentación se mantienen separados de los cambios funcionales para reducir riesgos sobre la versión jugable.

## 20. Conclusión técnica

El proyecto separa el cerebro del juego de su representación gráfica. Las reglas principales residen en C# y los procesos largos se ejecutan con concurrencia real, mientras Unity se mantiene como Vista e interfaz de usuario.

La solución demuestra de forma visible:

- POO;
- MVC;
- multithreading mediante Task/ThreadPool;
- sincronización;
- cancelación;
- colecciones concurrentes;
- System.IO;
- manejo de excepciones;
- networking opcional;
- pruebas automatizadas;
- integración gráfica con Unity.

Los entregables documentales restantes para el cierre son el UML final, el diagrama de flujo, las pruebas de escritorio formales y la evidencia del build/smoke test final.
