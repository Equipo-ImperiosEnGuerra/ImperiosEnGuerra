# Pruebas de escritorio finales — Imperios en Guerra

## 1. Objetivo

Este documento formaliza casos normales, inválidos, límite y concurrentes del proyecto final.

Evidencia automatizada validada:

```text
Total: 390
Correctas: 390
Errores: 0
Omitidas: 0
```

## 2. Matriz de casos

| ID | Caso | Tipo | Resultado esperado | Evidencia |
|---|---|---|---|---|
| PE-01 | Movimiento válido | Normal / concurrente | La unidad llega al destino progresivamente y libera su orden | Tests de movimiento concurrente y A* |
| PE-02 | Movimiento inválido | Inválido | Se rechaza sin modificar coordenadas | Tests de solicitudes inválidas |
| PE-03 | Cancelación de movimiento | Concurrente | Worker cancelado y unidad liberada | Tests de cancelación |
| PE-04 | Recolección con depósito | Normal / concurrente | Extrae, carga, vuelve al Centro Urbano y deposita | Tests de ciclo de recolección |
| PE-05 | Cancelar recolección | Concurrente | No pierde carga válida y vuelve a estado disponible | Tests de cancelación/recolección |
| PE-06 | Regenerar recurso agotado | Concurrente | Aparece nodo del mismo tipo en casilla válida | Tests de regeneración |
| PE-07 | Construcción válida | Normal / concurrente | Obra progresa y genera un edificio | Tests de construcción |
| PE-08 | Dos construcciones misma casilla | Race condition | Solo una obtiene la reserva | Tests de conflicto de construcción |
| PE-09 | Cancelar construcción | Concurrente | No queda edificio/casilla fantasma | Tests de cancelación |
| PE-10 | Entrenamiento en cola | Normal / concurrente | Respeta cola, costo y spawn seguro | Tests de entrenamiento |
| PE-11 | Gasto económico concurrente | Race condition | Solo una operación gana si el saldo alcanza para una | Tests de economía atómica |
| PE-12 | Operaciones distintas simultáneas | Concurrente | Pueden coexistir sin bloquear globalmente la partida | Tests de operaciones simultáneas |
| PE-13 | Ataque progresivo | Normal / concurrente | Reduce vida por intervalos hasta destruir | Tests de ataque concurrente |
| PE-14 | Cancelar ataque antes del impacto | Concurrente | No reduce vida | Tests de cancelación |
| PE-15 | Defensa reactiva IA | IA / concurrente | Responde la unidad exacta atacada, no las vecinas | `DefensaReactivaIaTests` |
| PE-16 | Paseo idle solo IA | IA | IA puede pasear; Aldeano humano queda quieto | Tests de paseo/reacción |
| PE-17 | Regla de victoria AND | Límite | Solo elimina facción sin CU y sin militares | `VictoriaFinalTests` |
| PE-18 | Finalización cancela workers | Concurrente | Se finaliza partida y se cancelan procesos pendientes | Tests de finalización |
| PE-19 | Archivos obligatorios | IO | Se generan configuración, log y resultado final | `ServicioArchivosTests` |
| PE-20 | WebSocket opcional | Integración | JSON válido se despacha; inválido se rechaza | `NetworkingTests` |

## 3. Casos de escritorio detallados

### PE-08 — Race condition de construcción

Estado inicial:

```text
Casilla X libre
Task A intenta construir en X
Task B intenta construir en X
```

Riesgo sin sincronización:

```text
A valida X libre
B valida X libre
A reserva X
B reserva X
=> dos construcciones superpuestas
```

Resultado requerido:

```text
Solo una operación obtiene la reserva.
La otra se rechaza.
No se duplican edificios.
```

### PE-11 — Gasto concurrente

Estado inicial:

```text
Saldo suficiente para una sola compra
Dos tareas intentan gastar simultáneamente
```

Riesgo:

```text
A lee saldo suficiente
B lee saldo suficiente
A descuenta
B descuenta
=> doble gasto
```

Solución:

`RecursosJugador.IntentarGastar(...)` protege comprobación y descuento con `lock`.

Resultado:

```text
Una operación tiene éxito.
La otra falla.
El saldo nunca queda negativo.
```

### PE-15 — Defensa reactiva

Estado inicial:

```text
Una IA puede tener un frente ofensivo normal.
El Humano golpea otro Soldado IA.
```

Resultado:

```text
La unidad exacta atacada puede contraatacar.
No se unen unidades cercanas.
No se duplican defensas de la misma facción.
```

### PE-17 — Regla AND

```text
Caso A:
CU existe
militares = 0
=> no termina

Caso B:
CU = 0
militares existen
=> no termina

Caso C:
CU = 0
militares = 0
=> facción eliminada
```

## 4. Validaciones manuales

| ID | Prueba | Resultado esperado | Estado |
|---|---|---|---|
| PM-01 | Menú principal | Visible y funcional | VALIDADO EN BUILD |
| PM-02 | Instrucciones | Legibles y retornan al menú | VALIDADO EN BUILD |
| PM-03 | Inicio de partida | Humano + 3 IAs + mapa | VALIDADO EN BUILD |
| PM-04 | Aldeano humano inactivo | Permanece quieto y HUD puede avisar | VALIDADO DURANTE DESARROLLO |
| PM-05 | Aldeano IA idle | Paseo corto sin afectar economía | VALIDADO DURANTE DESARROLLO |
| PM-06 | Recolección visual | Feedback visual y depósito | VALIDADO DURANTE DESARROLLO |
| PM-07 | Construcción visual | Obra progresa hasta edificio | VALIDADO DURANTE DESARROLLO |
| PM-08 | Entrenamiento visual | Unidad aparece tras progreso | VALIDADO DURANTE DESARROLLO |
| PM-09 | Combate / vida crítica | Vida cambia y <=25% se tiñe rojo | VALIDADO DURANTE DESARROLLO |
| PM-10 | Pausa | Detiene avance y puede reanudarse | VALIDADO DURANTE DESARROLLO |
| PM-11 | Derrota | Pantalla final y bloqueo de interacción | VALIDADO EN BUILD/UNITY |
| PM-12 | Build Windows | Ejecutable abre correctamente | VALIDADO |
| PM-13 | Launcher | Inicia API + juego y cierra API al salir | VALIDADO |
| PM-14 | Archivos | Los 3 txt se generan y contienen información | VALIDADO; log acumulativo debe limpiarse para evidencia final |
| PM-15 | Victoria Humana | Pantalla VICTORIA y bloqueo final | PENDIENTE EVIDENCIA VISUAL FINAL |

## 5. Conclusión

La suite automatizada cubre casos normales, inválidos, límite y concurrentes. Los riesgos principales de concurrencia se abordan mediante `lock`, `ConcurrentDictionary`, `ConcurrentQueue`, `Interlocked`, `SemaphoreSlim` y `CancellationToken`.

El cierre de evidencia manual se concentra en conservar un log limpio de la versión final y, si se desea evidencia visual completa, una captura de victoria humana.
