# Imperios en Guerra

Proyecto académico de Programación Orientada a Objetos desarrollado en C# y Unity.

**Imperios en Guerra** es un videojuego de estrategia en tiempo real (RTS) inspirado en Age of Empires. La modalidad actual del proyecto es **Humano vs Máquina**.

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
- escucha y envío de networking;
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

| Entidad | Vida | Daño | Alcance | Intervalo |
|---|---:|---:|---:|---:|
| Aldeano | 60 | 0 | 0 | — |
| Guerrero | 120 | 30 | 1 | 4 s |
| Lancero | 100 | 25 | 1 | 3.5 s |
| Arquero | 80 | 20 | 3 | 3 s |
| Monje | 70 | 15 | 2 | 5 s |
| Centro Urbano | 300 | — | — | — |

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
4. entrenar fuerza militar;
5. priorizar objetivos militares humanos;
6. aproximarse según el alcance de la unidad;
7. atacar;
8. asaltar el Centro Urbano cuando ya no hay objetivos militares y dispone de fuerza suficiente.

La Máquina no persigue Aldeanos como objetivo militar prioritario y mantiene un solo frente de combate activo.

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

## Networking

El proyecto implementa networking mediante **WebSocket + JSON**, manteniendo además la API REST utilizada por Unity.

Endpoint WebSocket:

```text
ws://localhost:5086/ws/partida
```

Estado de red:

```text
GET /api/red/estado
```

Tipos de mensajes soportados:

- `MOVER`
- `RECOLECTAR`
- `CONSTRUIR`
- `ENTRENAR`
- `ATACAR`

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

La suite completa fue validada localmente por el equipo al cierre de Fase 6 con **0 errores** después de las últimas estabilizaciones de combate, spawn, economía y UI.

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
- Fase 5: **TERMINADA E INTEGRADA EN DEVELOP**
- Fase 6: **TERMINADA, VALIDADA Y LISTA PARA MERGE A DEVELOP**

Documentación:
- `docs/FASE_5_AVANCES_Y_ESTADO_CERRADA.md`
- `docs/FASE_6_AVANCES_Y_ESTADO_CERRADA.md`
