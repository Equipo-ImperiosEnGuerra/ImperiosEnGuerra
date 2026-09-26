# Imperios en Guerra

Proyecto académico de Programación Orientada a Objetos desarrollado en C# y Unity.

**Imperios en Guerra** es un videojuego de estrategia en tiempo real (RTS) inspirado en Age of Empires. La modalidad final del proyecto es **1 jugador Humano vs 3 facciones controladas por Máquina**: Morada, Verde y Amarilla.

> **Alcance actualizado por el docente:** la guía original contemplaba dos jugadores en dos instancias y comunicación obligatoria entre ambas. Posteriormente ese alcance dejó de ser obligatorio para la entrega: la modalidad evaluada pasó a Humano vs Máquina y el networking quedó como componente opcional. El proyecto conserva la implementación de WebSocket + JSON como extensión técnica, pero no depende de un segundo cliente Unity para cumplir el alcance final.

## Tecnologías

- C#
- Unity
- Git y GitHub
- REST
- WebSockets
- JSON
- System.IO
- concurrencia con `Task`, ThreadPool, `CancellationToken` y primitivas thread-safe

## Versión de Unity

El proyecto utiliza **Unity 6000.6.0f1**.

Los integrantes del equipo deben utilizar la misma versión para reducir problemas de compatibilidad.

## Arquitectura

El proyecto utiliza **Modelo - Vista - Controlador (MVC)**:

- **Modelo:** clases C# POCO con estado, reglas y lógica del juego.
- **Vista:** Unity para escena, sprites, HUD, selección y representación visual.
- **Controlador:** comunica Vista/API con el Modelo y coordina las acciones.

La lógica importante no se concentra en `MonoBehaviour`.

## Concurrencia

Las operaciones largas se ejecutan fuera del Main Thread de Unity:

- movimiento progresivo;
- recolección orgánica;
- construcción progresiva;
- entrenamiento con cola;
- decisiones de la Máquina;
- escucha y envío de networking opcional;
- ataque concurrente.

Se utilizan `Task.Run`, ThreadPool, `CancellationToken`, `lock`, `ConcurrentDictionary`, `ConcurrentQueue`, `Interlocked` y `SemaphoreSlim` según la responsabilidad.

Los workers modifican el **Modelo C#**, nunca directamente `UnityEngine`. Unity consume estados/resultados desde su Main Thread.

Los tiempos del prototipo se mantienen configurables y el combate usa intervalos propios por tipo de unidad.

## Jugabilidad RTS

Actualmente están implementados:

- mapa lógico y recursos físicos de oro, madera y comida;
- Centro Urbano inicial y Aldeanos;
- movimiento progresivo con pathfinding;
- recolección: caminar → extraer → cargar → volver al Centro Urbano → depositar;
- construcción progresiva con reserva de costo/casilla, cancelación y reembolso;
- entrenamiento con costo, cola, progreso y spawn seguro;
- acciones concurrentes de varias unidades;
- una orden activa por unidad;
- combate con vida, daño, alcance e intervalo configurables;
- aproximación automática antes de atacar cuando el objetivo está fuera de alcance;
- ataque contra unidades y edificios enemigos;
- destrucción lógica y liberación de casillas;
- feedback visual de vida crítica;
- condición de victoria/derrota;
- costos económicos del prototipo centralizados en `ConfiguracionEconomia`.

La guía disponible no fija valores numéricos de vida, daño, armadura ni alcance por tipo de unidad. Por decisión explícita del equipo, Fase 6 incorpora un **balance propio del prototipo**, centralizado y documentado como tal.

## Balance de combate del prototipo

| Entidad | Vida | Daño ofensivo | Alcance de ataque | Intervalo de ataque |
|---|---:|---:|---:|---:|
| Aldeano | 60 | 0 | 0 | — |
| Guerrero | 120 | 30 | 1 | 4 s |
| Lancero | 100 | 25 | 1 | 3.5 s |
| Arquero | 80 | 20 | 3 | 3 s |
| Monje | 70 | 0 | 0 | — |
| Centro Urbano | 300 | 0 | 0 | — |

El **Monje no es una unidad ofensiva**. Su función es de apoyo: cura **15 puntos**, a **alcance 2**, con un intervalo de **5 s** entre curaciones.

El combate usa distancia de cuadrícula de 8 vecinos para el alcance, por lo que una diagonal inmediata cuenta como una casilla. El pathfinding continúa usando su movimiento ortogonal original.

## Combate y victoria

Los edificios tienen identidad `Guid` estable y pueden ser objetivo de ataque igual que las unidades.

Flujo base:

```text
unidad militar
→ seleccionar unidad o edificio enemigo
→ calcular si está en alcance
→ si hace falta, aproximarse automáticamente a una casilla válida
→ worker de ataque
→ aplicar daño
→ si Vida > 0, la entidad continúa viva
→ si Vida = 0, destruir y liberar la casilla
→ evaluar victoria
```

La Máquina mantiene un único frente militar activo a la vez para evitar ofensivas simultáneas demasiado rápidas, mientras el resto de la economía continúa siendo concurrente.

La regla terminal fue definida por el equipo como **AND**:

```text
sin Centros Urbanos
Y
sin unidades militares
= derrota
```

Perder únicamente el último Centro Urbano o únicamente la última unidad militar no termina la partida mientras todavía exista el otro conjunto.

Cuando la partida finaliza:

- se registra ganador y motivo en el Modelo;
- se rechazan nuevas órdenes;
- se cancelan workers concurrentes pendientes;
- se detiene el ciclo de IA;
- se genera `resultado_final.txt`;
- la API expone el estado final;
- Unity bloquea selección/clics sobre el mundo;
- aparece una pantalla completa de VICTORIA o DERROTA con botón SALIR.

Las entidades con **25% de vida o menos** se tiñen de rojo como feedback visual.

## Jugador Máquina

La Máquina utiliza las mismas reglas y servicios que el Humano.

Su ciclo de decisiones puede:

1. asignar Aldeanos a recursos;
2. entrenar Aldeanos para crecimiento;
3. construir un segundo Centro Urbano cuando dispone de recursos;
4. reunir recursos para una composición militar objetivo;
5. entrenar fuerza militar diversificada;
6. priorizar objetivos militares humanos;
7. aproximarse según el alcance de la unidad;
8. atacar;
9. asaltar el Centro Urbano cuando ya no hay objetivos militares y dispone de fuerza suficiente.

Las tres facciones usan una doctrina inicial distinta para evitar ejércitos idénticos:

- **Morada:** prioriza Arqueros y luego completa la composición.
- **Verde:** prioriza Lanceros y luego completa la composición.
- **Amarilla:** prioriza Guerreros y luego completa la composición.
- Cuando la economía está desarrollada, las facciones incorporan también Monjes como apoyo.

La IA no abandona su plan solo porque otra unidad sea más barata: puede ordenar a sus Aldeanos recolectar los recursos que necesita para la tropa objetivo. La Máquina no persigue Aldeanos como objetivo militar prioritario y mantiene un solo frente de combate activo.

## Economía visible y spawn

La escena de prueba distribuye recursos dejando corredores y casillas de interacción. Actualmente utiliza:

- 4 nodos de Oro;
- 4 nodos de Madera;
- 6 nodos de Comida;
- 14 recursos físicos en total.

El selector de entrenamiento muestra costos compactos:

- `O` = Oro;
- `M` = Madera;
- `C` = Comida.

El spawn de unidades entrenadas evita bordes cuando existe una alternativa interior y prioriza casillas con más salidas libres. Las marcas temporales de ocupación del spawn se liberan al abandonar la casilla para no dejar obstáculos fantasma.

## Networking opcional

La guía original exigía comunicación entre dos instancias de jugadores. **Ese requisito dejó de ser necesario para el alcance final indicado posteriormente por el docente**. La versión entregable funciona como Humano vs 3 Máquinas y no necesita un segundo cliente Unity.

Aun así, el repositorio conserva networking mediante **WebSocket + JSON** como extensión técnica y como demostración de escucha concurrente, manteniendo además la API REST utilizada por Unity.

Endpoint WebSocket:

```text
ws://localhost:5086/ws/partida
```

Estado de red:

```text
GET /api/red/estado
```

Tipos de mensajes soportados por el protocolo:

- `MOVER`
- `RECOLECTAR`
- `CONSTRUIR`
- `ENTRENAR`
- `ATACAR`
- `CURAR`

El listener WebSocket es asíncrono, mantiene clientes en una colección concurrente, usa `SemaphoreSlim` para serializar envíos por conexión, maneja cierres/errores y despacha las acciones hacia `ServicioAccionesConcurrentes`.

## Archivos obligatorios

La responsabilidad de `System.IO` está centralizada en `ServicioArchivos`.

Se generan:

- `configuracion.txt`;
- `log_partida.txt`;
- `resultado_final.txt`.

`resultado_final.txt` incluye estado final, regla de victoria, ganador, perdedor y motivo.

## Ejecutar la API

Desde la raíz del repositorio:

```bash
dotnet run --project src/ImperiosEnGuerra.Api/ImperiosEnGuerra.Api.csproj
```

## Pruebas

Suite .NET:

```bash
dotnet test tests/ImperiosEnGuerra.Tests/ImperiosEnGuerra.Tests.csproj
```

La suite se utiliza como validación principal del Modelo, servicios, concurrencia, networking opcional, ataques, IA y condición de victoria. Antes de la entrega final debe ejecutarse nuevamente desde `main` junto con las pruebas de Unity y el smoke test del build standalone.

Las advertencias de acceso denegado a `log_partida.txt` que aparecen en una prueba son intencionales: esa prueba verifica el manejo controlado de errores de IO.

## Flujo de Git

Ramas principales:

- `main`: versiones estables.
- `develop`: integración.
- ramas `feature/*`: trabajo por fase/tarea.

Flujo:

Issue → Branch → Desarrollo → Pruebas → Commit → Pull Request → Develop → Main

## Estado

- Fase 0: **TERMINADA**
- Fase 1: **TERMINADA**
- Fase 2: **TERMINADA**
- Fase 3: **TERMINADA**
- Fase 4: **TERMINADA**
- Etapa 4.5: **TERMINADA**
- Fase 5: **TERMINADA**
- Fase 6: **TERMINADA**
- Fase 7: **TERMINADA E INTEGRADA EN MAIN**
- Auditoría y documentación final: **EN DESARROLLO**

La versión estable actual incluye el cierre de Fase 7, optimización de snapshots para reducir micro-freezes, menú e instrucciones actualizados y estrategia militar diferenciada para las tres IAs.

Documentación relevante:
- `docs/FASE_5_AVANCES_Y_ESTADO_CERRADA.md`
- `docs/FASE_6_AVANCES_Y_ESTADO_CERRADA.md`
- `docs/BUILD_Y_EJECUCION.md`
- `docs/AUDITORIA_FINAL_GUIA.md`
