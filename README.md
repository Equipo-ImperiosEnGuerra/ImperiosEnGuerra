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
- escucha y envío de networking.

Se utilizan `Task.Run`, ThreadPool, `CancellationToken`, `lock`, `ConcurrentDictionary`, `ConcurrentQueue`, `Interlocked` y `SemaphoreSlim` según la responsabilidad.

Los workers modifican el **Modelo C#**, nunca directamente `UnityEngine`. Unity consume estados/resultados desde su Main Thread.

Los tiempos actuales del prototipo son configurables. En la API se usan como valores de demostración aproximadamente: movimiento 1 s, recolección 1 s, entrenamiento 5 s, construcción 7 s y ataque 1 s.

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
- intención de ataque validada para ambos jugadores;
- costos económicos del prototipo centralizados en `ConfiguracionEconomia`.

El daño, vida, destrucción y condición de victoria de combate se completarán en la siguiente fase.

## Jugador Máquina

La Fase 5 incorpora una Máquina que utiliza las mismas reglas y servicios que el Humano.

Su ciclo de decisiones puede:

1. asignar Aldeanos a recursos;
2. entrenar Aldeanos para crecimiento;
3. construir un segundo Centro Urbano cuando dispone de recursos;
4. entrenar una primera unidad militar;
5. aproximar unidades militares al enemigo;
6. preparar una intención de ataque cuando está adyacente.

El ciclo se ejecuta concurrentemente y es cancelable. No modifica Unity directamente ni utiliza operaciones especiales que eviten las validaciones del Modelo.

## Networking

La Fase 5 implementa networking mediante **WebSocket + JSON**, manteniendo además la API REST utilizada por Unity.

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

Formato general:

```json
{
  "tipo": "MOVER",
  "emisorId": "instancia-1",
  "mensajeId": "guid",
  "datos": {
    "unidadId": "guid",
    "destino": {
      "x": 4,
      "y": 3
    }
  }
}
```

El listener WebSocket es asíncrono, mantiene clientes en una colección concurrente, usa `SemaphoreSlim` para serializar envíos por conexión, maneja cierres/errores y despacha las acciones hacia `ServicioAccionesConcurrentes`.

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

Al cierre técnico de Fase 5 se validaron **313/313 pruebas correctas**.

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
- Fase 5: **IMPLEMENTADA Y VALIDADA EN FEATURE; PENDIENTE DE INTEGRACIÓN A DEVELOP**
- Fase 6: **PENDIENTE**

Detalle del cierre de Fase 5: `docs/FASE_5_AVANCES_Y_ESTADO_CERRADA.md`.
