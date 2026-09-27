# Diagrama de flujo final — Imperios en Guerra

Este documento representa el flujo real de la versión final del proyecto. Se separan el flujo general de la partida, el flujo de una acción concurrente y el ciclo de las IAs para hacer visible la arquitectura MVC y la concurrencia.

## 1. Flujo general de la partida

```mermaid
flowchart TD
    A[Inicio de aplicación] --> B[Menú inicial]
    B --> C{¿Iniciar partida?}
    C -- No --> Z[Salir]
    C -- Sí --> D[Unity envía configuración inicial a la API]

    D --> E[InicializadorPartida crea mapa compartido]
    E --> F[Crear Humano + 3 IAs]
    F --> G[Crear Centros Urbanos, Aldeanos y recursos físicos]
    G --> H[ServicioArchivos genera configuracion.txt]
    H --> I[EstadoPartidaService establece Partida]
    I --> J[ServicioSesionJuego activa sesión]

    J --> K1[Iniciar ciclo IA]
    J --> K2[Iniciar reacciones automáticas]
    J --> K3[Iniciar regeneración de recursos]
    J --> K4[Unity comienza sincronización de snapshots]

    K1 --> L[Partida activa]
    K2 --> L
    K3 --> L
    K4 --> L

    L --> M{Entrada o evento}
    M -->|Mover| N[Movimiento concurrente]
    M -->|Recolectar| O[Recolección concurrente]
    M -->|Construir| P[Construcción concurrente]
    M -->|Entrenar| Q[Entrenamiento concurrente]
    M -->|Atacar| R[Ataque concurrente]
    M -->|Curar| S[Curación concurrente]
    M -->|Pausa| T[Pausa temporal]
    M -->|Decisión IA| U[Acción IA]

    N --> V[Actualizar Modelo]
    O --> V
    P --> V
    Q --> V
    R --> V
    S --> V
    U --> V

    T --> T1[Detener IA, reacciones y regeneración]
    T1 --> T2[Workers de gameplay esperan en puerta de pausa]
    T2 --> T3{¿Reanudar?}
    T3 -- Sí --> J
    T3 -- No / salir --> Y[Cancelar procesos y cerrar sesión]

    V --> W[Registrar evento en log_partida.txt]
    W --> X{¿Condición terminal?}

    X -- No --> L
    X -- Sí --> AA[Partida.IntentarFinalizar]
    AA --> AB[Guardar resultado_final.txt]
    AB --> AC[Cancelar workers pendientes]
    AC --> AD[Detener IA, reacciones y regeneración]
    AD --> AE[Unity recibe snapshot final]
    AE --> AF[Bloquear interacción]
    AF --> AG[Mostrar VICTORIA o DERROTA]
    AG --> AH{¿Volver al menú o salir?}
    AH -->|Menú| B
    AH -->|Salir| Z
```

## 2. Flujo de una acción concurrente

Este flujo aplica al patrón general usado por movimiento, recolección, construcción, entrenamiento, ataque y curación.

```mermaid
flowchart TD
    A[Jugador pulsa una acción en Unity] --> B[ControladorAcciones]
    B --> C[ControladorConexionApi]
    C --> D[Petición REST a la API]

    D --> E[Validar request]
    E --> F{¿Request válido?}

    F -- No --> G[Resultado rechazado]
    G --> H[Registrar evento]
    H --> I[Unity muestra mensaje de error]

    F -- Sí --> J[ServicioAccionesConcurrentes]
    J --> K[GestorProcesosConcurrentes.Iniciar]
    K --> L[Task.Run / ThreadPool]
    L --> M[Worker obtiene CancellationToken]

    M --> N{¿Cancelado?}
    N -- Sí --> O[Publicar resultado Cancelado]
    N -- No --> P[Preparar / validar operación en Modelo]

    P --> Q{¿Puede ejecutarse?}
    Q -- No --> R[Resultado fallido]
    R --> S[ConcurrentQueue / resultado por Id]

    Q -- Sí --> T[Aplicar paso o progreso]
    T --> U[EstadoPartidaService lock]
    U --> V[Modificar Modelo C#]
    V --> W{¿Proceso terminado?}

    W -- No --> X[Esperar intervalo]
    X --> N

    W -- Sí --> Y[Completar orden de unidad]
    Y --> Z[Publicar resultado Completado]
    Z --> S

    S --> AA[Unity consulta resultado / snapshot]
    AA --> AB[Main Thread de Unity]
    AB --> AC[VistaPartida / VistaHud]
```

### Punto clave

El worker secundario nunca modifica directamente:

```text
GameObject
Transform
SpriteRenderer
Canvas
Text
Button
u otro objeto UnityEngine
```

La modificación gráfica ocurre cuando Unity recibe el resultado o el snapshot y lo representa en su Main Thread.

## 3. Flujo de recolección

```mermaid
flowchart TD
    A[Seleccionar Aldeano] --> B[Seleccionar nodo de recurso]
    B --> C[Validar Aldeano y recurso]
    C --> D{¿Recurso disponible?}

    D -- No --> E[Rechazar acción]
    D -- Sí --> F[Worker de recolección]

    F --> G[Calcular aproximación]
    G --> H[Mover Aldeano progresivamente]
    H --> I[Extraer recurso]
    I --> J{¿Carga llena o recurso agotado?}

    J -- No --> I
    J -- Sí --> K[Buscar Centro Urbano aliado]
    K --> L[Mover al depósito]
    L --> M[Vaciar carga]
    M --> N[Agregar saldo al jugador]
    N --> O[Registrar RECOLECCION_DEPOSITO]

    O --> P{¿Recurso sigue disponible y orden sigue activa?}
    P -- Sí --> G
    P -- No --> Q[Finalizar recolección]

    I --> R{¿Nodo agotado?}
    R -- Sí --> S[ServicioRegeneracionRecursos detecta agotado]
    S --> T[Esperar retardo de regeneración]
    T --> U[Buscar casilla libre]
    U --> V[Crear nuevo nodo del mismo tipo]
    V --> W[Registrar RECURSO_REGENERADO]
```

## 4. Flujo de construcción

```mermaid
flowchart TD
    A[Seleccionar Aldeano] --> B[Elegir Construir]
    B --> C[Seleccionar destino]
    C --> D[Validar coordenada, ocupación y costo]

    D --> E{¿Puede construirse?}
    E -- No --> F[Rechazar]
    E -- Sí --> G[Reservar costo y casilla]

    G --> H[Worker de construcción]
    H --> I[Crear ObraConstruccion]
    I --> J[Avanzar progreso con tiempo]
    J --> K{¿100%?}

    K -- No --> J
    K -- Sí --> L[Crear edificio lógico]
    L --> M[Eliminar obra]
    M --> N[Unity recibe snapshot]
    N --> O[Representar edificio terminado]

    H --> P{¿Cancelación / fallo?}
    P -- Sí --> Q[Limpiar reserva]
    Q --> R[Reembolsar cuando corresponde]
```

## 5. Flujo de entrenamiento

```mermaid
flowchart TD
    A[Seleccionar Centro Urbano] --> B[Elegir tipo de unidad]
    B --> C[Validar costo]
    C --> D{¿Hay recursos?}

    D -- No --> E[Rechazar]
    D -- Sí --> F[Descontar costo]
    F --> G[Encolar EntrenamientoPendiente]

    G --> H{¿Está al frente de la cola?}
    H -- No --> I[Esperar turno]
    I --> H

    H -- Sí --> J[Worker avanza progreso]
    J --> K{¿100%?}
    K -- No --> J

    K -- Sí --> L[Buscar spawn seguro]
    L --> M{¿Hay casilla válida?}
    M -- No --> N[Esperar / reintentar según operación]
    N --> L

    M -- Sí --> O[FabricaUnidades crea Unidad]
    O --> P[Agregar Unidad al Jugador]
    P --> Q[Retirar entrenamiento de la cola]
    Q --> R[Unity representa nueva unidad]
```

## 6. Flujo de combate y victoria

```mermaid
flowchart TD
    A[Seleccionar unidad militar] --> B[Seleccionar objetivo enemigo]
    B --> C[Validar atacante y objetivo]

    C --> D{¿Objetivo en alcance?}
    D -- No --> E[Calcular aproximación]
    E --> F[Mover progresivamente]
    F --> D

    D -- Sí --> G[Worker de ataque]
    G --> H[Esperar intervalo de ataque]
    H --> I[Aplicar daño bajo sincronización]

    I --> J{¿Objetivo destruido?}
    J -- No --> K[Publicar impacto]
    K --> L{¿Objetivo es Soldado IA atacado por Humano?}
    L -- Sí --> M[Intentar defensa reactiva con esa misma unidad]
    L -- No --> H
    M --> H

    J -- Sí --> N[Eliminar entidad y liberar casilla]
    N --> O[EvaluadorVictoria]

    O --> P{¿Facción sin Centros Urbanos Y sin militares?}
    P -- No --> Q[Continuar partida]
    P -- Sí --> R{¿Era una IA?}

    R -- Sí --> S{¿Las 3 IAs están eliminadas?}
    S -- No --> Q
    S -- Sí --> T[Ganador = Humano]

    R -- No --> U[Ganador = IA activa]

    T --> V[Finalizar Partida]
    U --> V
    V --> W[Guardar resultado_final.txt]
    W --> X[Cancelar workers]
    X --> Y[Mostrar pantalla final]
```

## 7. Ciclo de decisión de cada IA

```mermaid
flowchart TD
    A[ServicioJugadorMaquina activo] --> B[Task.Run: ciclo de IA]
    B --> C[Recorrer las 3 facciones]

    C --> D{¿Frente militar normal activo?}
    D -- No --> E[Preparar decisión militar]
    D -- Sí --> F[No iniciar otro frente ofensivo]

    E --> G{¿Combate habilitado por tiempo de gracia?}
    G -- Sí --> H[Patrullar / aproximar / atacar]
    G -- No --> F

    H --> I[Máximo 1 frente ofensivo normal por IA]
    F --> J[Preparar decisión económica]
    I --> J

    J --> K{¿Qué necesita la facción?}
    K -->|Recursos| L[Recolectar]
    K -->|Crecimiento| M[Entrenar Aldeano]
    K -->|Infraestructura| N[Construir Centro Urbano]
    K -->|Ejército| O[Entrenar unidad según doctrina]

    L --> P[Máximo 1 recolección principal por IA]
    M --> Q[Ejecutar worker]
    N --> Q
    O --> Q
    P --> Q

    Q --> R[Intentar paseo ambiental]
    R --> S{¿Hay Aldeano IA libre?}
    S -- Sí --> T[Mover 1 casilla ortogonal con baja prioridad]
    S -- No --> U[Sin paseo]

    T --> V[Esperar siguiente intervalo de decisión]
    U --> V
    V --> C

    W[Impacto humano sobre Soldado IA] --> X{¿Hay defensa reactiva disponible?}
    X -- Sí --> Y[Contraatacar con la unidad exacta atacada]
    X -- No --> Z[No crear frente extra]
```

## 8. Flujo de pausa

```mermaid
flowchart TD
    A[Jugador abre menú de pausa] --> B[Unity bloquea interacción visual]
    B --> C[POST /api/sesion/pausar-temporal]
    C --> D[ServicioSesionJuego marca pausa temporal]
    D --> E[ServicioAccionesConcurrentes cierra puerta de pausa]
    D --> F[Detener ciclo IA]
    D --> G[Detener reacciones automáticas]
    D --> H[Detener regeneración]

    E --> I[Workers existentes conservan estado y esperan]
    F --> I
    G --> I
    H --> I

    I --> J{¿Reanudar?}
    J -- Sí --> K[POST /api/sesion/reanudar]
    K --> L[Abrir puerta de pausa]
    K --> M[Reiniciar IA, reacciones y regeneración]
    L --> N[Continuar partida]
    M --> N

    J -- No / salir --> O[Pausa de sesión]
    O --> P[Cancelar workers]
    P --> Q[Detener servicios]
```

## 9. Relación con MVC y concurrencia

El flujo general puede resumirse como:

```text
Vista Unity
    ↓ entrada
Controlador
    ↓ request
API / Servicios
    ↓
Task.Run / ThreadPool
    ↓
Modelo C# protegido con sincronización
    ↓
resultado thread-safe
    ↓
API / snapshot
    ↓
Controlador Unity
    ↓
Vista Unity
```

Esto demuestra que:

1. la lógica del juego permanece en C#;
2. Unity actúa principalmente como Vista;
3. los controladores sirven de puente;
4. las operaciones largas se ejecutan de forma concurrente;
5. los datos compartidos están sincronizados;
6. los workers no modifican UnityEngine;
7. la partida puede cancelar o pausar procesos sin bloquear la interfaz.
