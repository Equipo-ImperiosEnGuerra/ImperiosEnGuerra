# Fase 4 - Concurrencia, sincronizacion y cancelacion

## Estado

EN DESARROLLO / cierre tecnico pendiente de verificacion final.

## Objetivo

Demostrar concurrencia real en C# para las operaciones temporales del juego sin modificar `UnityEngine` desde hilos secundarios.

Flujo aplicado:

```text
Unity Main Thread
    -> API
    -> Task.Run / ThreadPool
    -> Modelo C#
    -> resultado thread-safe
    -> API
    -> Unity Main Thread
    -> Vista / HUD
```

Las Coroutines de Unity se usan solo para HTTP, polling y actualizacion visual. No sustituyen el multihilo requerido.

## Procesos concurrentes implementados

Se usa `Task.Run` a traves de `GestorProcesosConcurrentes` para:

- movimiento;
- recoleccion;
- construccion;
- entrenamiento.

La decision de usar `Task` se tomo porque estas operaciones son trabajos temporales, cancelables y faciles de probar sin acoplarse a Unity.

## Cancelacion

Cada proceso recibe un `CancellationToken`.

`GestorProcesosConcurrentes` mantiene los `CancellationTokenSource` activos en un `ConcurrentDictionary<Guid, CancellationTokenSource>`.

Se soporta:

- cancelar un proceso por ID;
- cancelar todos los procesos activos;
- cancelar procesos al iniciar una nueva partida para evitar trabajo pendiente asociado al estado anterior.

El retardo configurado en desarrollo mediante `Concurrencia:RetardoDemostracionMs` existe solo para hacer observable la concurrencia y la cancelacion. No representa velocidad, tiempo de construccion ni otra regla de gameplay.

## Datos compartidos y race conditions

### RecursosJugador

Riesgo:

```text
Task A lee saldo
Task B lee el mismo saldo
Task A escribe
Task B escribe
=> se puede perder una actualizacion
```

Proteccion aplicada:

- `lock` en lectura, suma, consulta de pago y gasto.

Pruebas:

- multiples Tasks agregan recursos sin perder incrementos;
- multiples Tasks gastan sin producir saldo negativo.

### Estado de la partida

Movimiento, construccion, entrenamiento y otras operaciones pueden consultar o modificar el mismo estado.

Proteccion aplicada:

- `EstadoPartidaService` serializa las secciones criticas mediante `lock`.

Ejemplo demostrado:

- dos construcciones pueden iniciarse concurrentemente e intentar la misma casilla;
- solo una entra con exito al cambio de estado;
- la segunda observa la casilla ya ocupada y es rechazada.

La prueba se repite varias veces para ayudar a detectar condiciones de carrera.

### Resultados de procesos

Riesgo inicial:

- varios pollers de Unity podian consultar una cola global y consumir el resultado de otro proceso.

Proteccion aplicada:

- `ConcurrentQueue<ResultadoProcesoConcurrente>` para publicacion thread-safe;
- `ConcurrentDictionary<Guid, ResultadoProcesoConcurrente>` para recuperar un resultado por `procesoId`;
- cada Coroutine de Unity consulta exclusivamente su propio proceso.

## Comunicacion con Unity

Los workers no llaman directamente a:

- `GameObject`;
- `Transform`;
- `SpriteRenderer`;
- `Text`;
- otros objetos de `UnityEngine`.

El worker produce un resultado C# puro. Unity lo consulta posteriormente desde su Main Thread y entonces:

- refresca `VistaPartida`;
- actualiza `VistaHud`;
- muestra mensajes de exito o error.

Existe una prueba estructural que verifica que la capa `ImperiosEnGuerra.Servicios.Concurrencia` no tenga referencia a `UnityEngine`.

## Simultaneidad visible

Unity ya no bloquea globalmente movimiento, recoleccion, construccion y entrenamiento.

Se permite una operacion activa de cada tipo al mismo tiempo. Por ejemplo:

```text
Movimiento      -> activo
Recoleccion     -> activa
Construccion    -> activa
Entrenamiento   -> activo
```

Una segunda operacion del mismo tipo permanece bloqueada como medida de control en esta fase.

Ataque permanece incompatible con otras acciones mientras el combate completo no esta definido.

## Que comparten los Tasks

Los procesos pueden compartir:

- la instancia de `Partida`;
- jugadores;
- mapa y casillas;
- colecciones de unidades y edificios;
- saldos de `RecursosJugador`.

La sincronizacion se concentra en servicios/modelo y no en `MonoBehaviour`.

## Pruebas relevantes

La fase incluye pruebas para:

- dos workers activos al mismo tiempo;
- movimiento y entrenamiento simultaneos;
- suma concurrente de recursos;
- gasto concurrente sin saldo negativo;
- cancelacion individual;
- cancelacion global;
- construccion concurrente sobre la misma casilla;
- entrenamiento concurrente sobre la misma casilla de aparicion;
- recuperacion de resultados por ID sin cruces;
- ejecuciones repetidas para detectar races;
- ausencia de referencia a `UnityEngine` desde la capa concurrente;
- pruebas EditMode de los contratos y controladores Unity.

## Pendientes fuera del alcance de esta fase

No se inventaron:

- tiempos reales de movimiento;
- tiempos reales de construccion;
- tiempos reales de entrenamiento;
- cantidad o ritmo de recoleccion;
- costos de unidades o edificios.

Esos valores solo deben incorporarse cuando los requisitos los definan.

Networking/IA completa pertenece a una fase posterior.
