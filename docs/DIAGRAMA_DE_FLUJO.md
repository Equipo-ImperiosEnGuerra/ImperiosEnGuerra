# Diagrama de Flujo — Imperios en Guerra

## 1. Flujo general de la partida

```mermaid
flowchart TD
    A[Inicio] --> B[Menú inicial]
    B --> C{Iniciar partida}
    C -- No --> Z[Salir]
    C -- Sí --> D[Unity solicita configuración]
    D --> E[Inicializar mapa 15x15]
    E --> F[Crear Humano + 3 IAs]
    F --> G[Crear bases, Aldeanos y recursos]
    G --> H[Guardar configuracion.txt]
    H --> I[Establecer Partida]
    I --> J[Activar sesión]

    J --> K1[Ciclo IA]
    J --> K2[Reacciones automáticas]
    J --> K3[Regeneración de recursos]
    J --> K4[Sincronización de snapshots]

    K1 --> L[Partida activa]
    K2 --> L
    K3 --> L
    K4 --> L

    L --> M{Acción}
    M -->|Mover| N[Worker movimiento]
    M -->|Recolectar| O[Worker recolección]
    M -->|Construir| P[Worker construcción]
    M -->|Entrenar| Q[Worker entrenamiento]
    M -->|Atacar| R[Worker ataque]
    M -->|Curar| S[Worker curación]
    M -->|IA| T[Decisión IA]

    N --> U[Actualizar Modelo]
    O --> U
    P --> U
    Q --> U
    R --> U
    S --> U
    T --> U

    U --> V[Registrar log_partida.txt]
    V --> W{Condición terminal}
    W -- No --> L
    W -- Sí --> X[Finalizar Partida]
    X --> Y[Guardar resultado_final.txt]
    Y --> Y2[Cancelar workers y detener servicios]
    Y2 --> Y3[Unity muestra VICTORIA o DERROTA]
    Y3 --> Z
```

## 2. Flujo de una acción concurrente

```mermaid
flowchart TD
    A[Entrada Unity] --> B[Controlador]
    B --> C[Petición REST]
    C --> D[Validación]
    D --> E{Válida}
    E -- No --> F[Resultado rechazado]
    E -- Sí --> G[ServicioAccionesConcurrentes]
    G --> H[GestorProcesosConcurrentes]
    H --> I[Task.Run / ThreadPool]
    I --> J[CancellationToken]
    J --> K{Cancelado}
    K -- Sí --> L[Resultado Cancelado]
    K -- No --> M[Preparar operación]
    M --> N{Puede ejecutarse}
    N -- No --> O[Resultado fallido]
    N -- Sí --> P[EstadoPartidaService / lock]
    P --> Q[Modificar Modelo]
    Q --> R{Terminó}
    R -- No --> S[Esperar intervalo]
    S --> J
    R -- Sí --> T[Publicar resultado]
    T --> U[Unity recibe snapshot]
    U --> V[Actualizar Vista en Main Thread]
```

## 3. Recolección

```mermaid
flowchart TD
    A[Seleccionar Aldeano y recurso] --> B[Validar]
    B --> C[Aproximarse]
    C --> D[Extraer]
    D --> E{Carga llena o recurso agotado}
    E -- No --> D
    E -- Sí --> F[Volver al Centro Urbano]
    F --> G[Depositar]
    G --> H[Agregar saldo]
    H --> I{Continuar}
    I -- Sí --> C
    I -- No --> J[Finalizar]

    D --> K{Nodo agotado}
    K -- Sí --> L[Regeneración en segundo plano]
    L --> M[Buscar casilla válida]
    M --> N[Crear nodo del mismo tipo]
```

## 4. Construcción

```mermaid
flowchart TD
    A[Seleccionar Aldeano y destino] --> B[Validar costo y casilla]
    B --> C{Puede construir}
    C -- No --> D[Rechazar]
    C -- Sí --> E[Reservar costo y casilla]
    E --> F[Crear obra]
    F --> G[Avanzar progreso]
    G --> H{100 por ciento}
    H -- No --> G
    H -- Sí --> I[Crear edificio]
    I --> J[Retirar obra]
```

## 5. Entrenamiento

```mermaid
flowchart TD
    A[Seleccionar Centro Urbano] --> B[Elegir unidad]
    B --> C[Validar costo]
    C --> D{Recursos suficientes}
    D -- No --> E[Rechazar]
    D -- Sí --> F[Descontar y encolar]
    F --> G[Esperar turno]
    G --> H[Avanzar progreso]
    H --> I{100 por ciento}
    I -- No --> H
    I -- Sí --> J[Buscar spawn seguro]
    J --> K[Crear Unidad]
    K --> L[Agregar al Jugador]
```

## 6. Combate y victoria

```mermaid
flowchart TD
    A[Seleccionar militar y enemigo] --> B[Validar]
    B --> C{En alcance}
    C -- No --> D[Aproximarse]
    D --> C
    C -- Sí --> E[Worker de ataque]
    E --> F[Esperar intervalo]
    F --> G[Aplicar daño]
    G --> H{Objetivo destruido}
    H -- No --> F
    H -- Sí --> I[Eliminar entidad]
    I --> J[Evaluar victoria]
    J --> K{Sin Centros Urbanos Y sin militares}
    K -- No --> L[Continuar]
    K -- Sí --> M{Las 3 IAs eliminadas}
    M -- Sí --> N[Victoria Humana]
    M -- No --> O[Continuar o derrota según facción]
```

## 7. IA

```mermaid
flowchart TD
    A[Ciclo IA] --> B[Recorrer facciones]
    B --> C{Frente normal activo}
    C -- No --> D[Preparar acción militar]
    C -- Sí --> E[Mantener frente existente]
    D --> F[Máximo 1 frente ofensivo normal]
    E --> G[Preparar economía]
    F --> G
    G --> H{Necesidad}
    H -->|Recursos| I[Recolectar]
    H -->|Crecimiento| J[Entrenar Aldeano]
    H -->|Ejército| K[Entrenar según doctrina]
    I --> L[Worker]
    J --> L
    K --> L
    L --> M[Intentar paseo idle]
    M --> N[Esperar siguiente ciclo]

    O[Soldado IA recibe impacto humano] --> P{Defensa disponible}
    P -- Sí --> Q[Contraatacar con esa misma unidad]
    P -- No --> R[No abrir frente adicional]
```

## 8. Pausa

```mermaid
flowchart TD
    A[Abrir pausa] --> B[Bloquear interacción]
    B --> C[Pausar sesión]
    C --> D[Detener IA/reacciones/regeneración]
    C --> E[Workers esperan]
    D --> F{Reanudar}
    E --> F
    F -- Sí --> G[Reactivar servicios]
    G --> H[Continuar partida]
    F -- No --> I[Cancelar y salir]
```

## 9. Relación MVC + concurrencia

```text
Vista Unity
    ↓
Controlador
    ↓
API / Servicios
    ↓
Task.Run / ThreadPool
    ↓
Modelo C# sincronizado
    ↓
resultado thread-safe
    ↓
API / snapshot
    ↓
Controlador Unity
    ↓
Vista Unity
```
