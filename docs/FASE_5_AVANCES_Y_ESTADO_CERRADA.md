# FASE 5 — AVANCES Y ESTADO DE CIERRE

**Proyecto:** Imperios en Guerra  
**Modalidad actual:** Humano vs Máquina  
**Rama:** `feature/fase5-maquina-networking`  
**Estado técnico:** TERMINADO Y VALIDADO  
**Integración:** pendiente de Pull Request hacia `develop`  
**Validación final supervisada:** 313/313 pruebas .NET correctas

---

## 1. Objetivo de la fase

La Fase 5 tuvo dos objetivos principales:

1. implementar un Jugador Máquina real que reutilizara las mismas reglas del Humano;
2. resolver el requisito pendiente de networking sin eliminarlo por el cambio de modalidad a Humano vs Máquina.

Se mantuvo MVC y la regla central del proyecto:

```text
C# / Modelo = reglas y estado
Controladores/Servicios = coordinación
Unity = Vista
```

Los workers secundarios no modifican `UnityEngine`.

---

## 2. F5-01 — mismas operaciones para Humano y Máquina

Se eliminó el acoplamiento que asumía que todas las acciones pertenecían al `JugadorHumano`.

`Partida` ahora puede resolver propietarios de unidades/edificios y obtener al oponente.

Las siguientes operaciones funcionan para cualquiera de los dos participantes:

- movimiento;
- recolección;
- depósito;
- construcción;
- entrenamiento;
- intención de ataque.

La Máquina utiliza los mismos costos, validaciones, mapas, colas, reservas, cancelaciones y workers que el Humano.

### Resultado

**TERMINADO**

---

## 3. F5-02/F5-03 — ciclo de decisiones y recolección autónoma

Se añadieron:

- `DecisionMaquina`;
- `PlanificadorDecisionMaquina`;
- `ServicioJugadorMaquina`.

El planificador es lógica C# sin Unity. El servicio de Máquina ejecuta un ciclo mediante `Task` y `CancellationTokenSource`.

Primera conducta autónoma:

```text
Aldeano disponible
→ seleccionar recurso físico no agotado cercano
→ ServicioAccionesConcurrentes
→ caminar
→ extraer
→ regresar
→ depositar
```

El servicio impide asignar repetidamente una segunda orden estratégica a una unidad que ya tiene un proceso activo.

### Resultado

**TERMINADO**

---

## 4. F5-04/F5-05 — construcción y entrenamiento estratégicos

Se amplió la política de decisión.

Prioridad actual del prototipo:

1. si la Máquina tiene menos de 3 Aldeanos y puede pagar uno, entrena un Aldeano;
2. si tiene economía suficiente y un solo Centro Urbano, intenta construir un segundo Centro Urbano;
3. si no tiene unidad militar y puede pagar un Guerrero, entrena uno;
4. si ninguna prioridad anterior aplica, continúa con economía/recolección.

Estos valores y prioridades son balance propio del prototipo académico; no se presentan como valores oficiales de Age of Empires.

La IA busca una casilla válida para expansión y evita:

- recursos físicos;
- unidades;
- edificios;
- obras;
- posiciones no transitables;
- destinos sin acceso adyacente razonable.

Construcción y entrenamiento siguen pasando por `ServicioAccionesConcurrentes`.

### Resultado

**TERMINADO**

---

## 5. F5-06/F5-07 — movimiento militar y ataque autónomo

Se añadieron decisiones `Mover` y `Atacar`.

Flujo actual:

```text
unidad militar disponible
→ localizar unidad humana cercana
→ si está lejos, aproximarse
→ si está adyacente, preparar ataque
```

El movimiento se ejecuta mediante el pathfinding y worker existentes.

El ataque solamente valida/prepara la intención. **No se inventaron estadísticas de combate en esta fase.**

Quedan para Fase 6:

- vida;
- daño;
- rango definitivo;
- destrucción;
- muerte de unidades;
- condición de victoria.

### Resultado

**TERMINADO**

---

## 6. F5-08/F5-09 — networking WebSocket + JSON

Se mantuvo el requisito de networking porque la documentación disponible del profesor no indica que haya sido eliminado.

Se implementó servidor WebSocket en la API.

### Endpoint

```text
/ws/partida
```

Con la configuración local usada por el proyecto:

```text
ws://localhost:5086/ws/partida
```

### Estado REST

```text
GET /api/red/estado
```

### Mensaje estructurado

Formato general:

```json
{
  "tipo": "MOVER",
  "emisorId": "instancia-1",
  "mensajeId": "guid",
  "datos": {}
}
```

Tipos soportados:

- `MOVER`;
- `RECOLECTAR`;
- `CONSTRUIR`;
- `ENTRENAR`;
- `ATACAR`.

### Arquitectura de red

```text
Cliente / instancia
      ↓ JSON
WebSocket
      ↓
ServicioRedPartida
      ↓
DespachadorMensajesRed
      ↓
ServicioAccionesConcurrentes
      ↓
Modelo C#
```

### Concurrencia de red

`ServicioRedPartida` utiliza:

- `Task`/async;
- `CancellationToken`;
- `ConcurrentDictionary` para conexiones;
- `SemaphoreSlim` por conexión para evitar envíos simultáneos sobre un mismo socket.

Se manejan:

- cierre normal;
- cancelación;
- `WebSocketException`;
- JSON inválido;
- tipo de mensaje no soportado;
- desconexión del cliente.

### Resultado

**TERMINADO**

---

## 7. Pruebas

La fase fue validada progresivamente.

Baselines supervisadas durante el desarrollo:

```text
291/291
292/292
294/294
299/299
304/304
306/306
313/313
```

Validación final reportada:

```text
Total: 313
Correctas: 313
Errores: 0
Omitidas: 0
```

Se cubrieron, entre otros:

- movimiento de Máquina;
- recolección y depósito de Máquina;
- construcción inmediata/progresiva;
- entrenamiento inmediato/concurrente;
- ataque Máquina → Humano;
- planificación autónoma;
- inicio/detención del ciclo;
- economía estratégica;
- expansión;
- entrenamiento estratégico;
- aproximación militar;
- ataque autónomo;
- despacho JSON de las cinco acciones de red;
- mensajes de red inválidos.

---

## 8. Concurrencia explicable en sustentación

Después de esta fase existen varios trabajos concurrentes claramente demostrables:

- workers de movimiento;
- workers de recolección;
- workers de construcción;
- workers de entrenamiento;
- worker/ciclo de decisiones de la Máquina;
- escucha WebSocket;
- envíos WebSocket.

Datos compartidos relevantes:

- estado de `Partida`;
- saldos de `RecursosJugador`;
- órdenes de `Unidad`;
- cola de entrenamiento;
- obras;
- procesos concurrentes;
- conexiones de red.

Mecanismos principales de sincronización:

- `lock`;
- `ConcurrentDictionary`;
- `ConcurrentQueue`;
- `Interlocked`;
- `SemaphoreSlim`;
- `CancellationToken`.

---

## 9. Commits principales de la fase

```text
9da3070 refactor: permitir movimiento para ambos jugadores
8688bdf refactor: permitir recoleccion para ambos jugadores
4961d21 refactor: permitir construcción para ambos jugadores
1834c8a fix: restaurar reserva de entrenamiento mientras se generaliza construcción
dcd67e6 fix: cobrar construcción directa al propietario correcto
1f78eb8 refactor: permitir entrenamiento para ambos jugadores
68ff65e refactor: permitir intención de ataque para ambos jugadores
f8d1a48 feat: agregar ciclo de decisiones y recolección autónoma de la máquina
813fafa feat: agregar construcción y entrenamiento estratégicos de la máquina
0c08233 feat: agregar movimiento estratégico y ataque autónomo de la máquina
f1a3c73 feat: implementar networking WebSocket con mensajes JSON
```

---

## 10. Estado de cierre

```text
FASE 0      TERMINADA
FASE 1      TERMINADA
FASE 2      TERMINADA
FASE 3      TERMINADA
FASE 4      TERMINADA
ETAPA 4.5   TERMINADA
FASE 5      TERMINADA TÉCNICAMENTE
FASE 6      PENDIENTE
```

Fase 5 queda lista para:

```text
Pull Request
→ revisión
→ merge a develop
→ Fase 6
```

La siguiente fase debe concentrarse en combate real y condición de victoria sin deshacer las reglas de arquitectura/concurrencia establecidas.
