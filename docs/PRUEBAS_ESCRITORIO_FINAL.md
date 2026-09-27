# Pruebas de escritorio finales — Imperios en Guerra

## 1. Objetivo

Este documento formaliza casos normales, inválidos, límite y concurrentes del proyecto final.

La evidencia automatizada se apoya en la suite .NET ejecutada sobre `main` después de los últimos hotfixes:

```text
Total: 390
Correctas: 390
Errores: 0
Omitidas: 0
```

Las pruebas visuales de Unity y el smoke test del build se mantienen separados porque requieren ejecución gráfica manual.

## 2. Convenciones

- **APROBADA — AUTOMATIZADA:** existe prueba automatizada y la suite final pasó.
- **PENDIENTE — MANUAL:** la lógica existe, pero la evidencia final debe recogerse ejecutando Unity/build.
- **Normal:** caso esperado de uso.
- **Inválido:** entrada rechazada sin corromper el estado.
- **Límite:** condición extrema o frontera.
- **Concurrente:** intervienen múltiples workers o acceso compartido.

---

## PE-01 — Movimiento válido

**Tipo:** Normal / concurrente

**Estado inicial**

```text
Partida activa
Unidad humana disponible
Origen = casilla A
Destino = casilla B transitable
No existe orden activa
```

**Pasos de escritorio**

1. El jugador selecciona una unidad.
2. Solicita movimiento a B.
3. `ServicioAccionesConcurrentes` crea un worker.
4. Se calcula la ruta.
5. El worker espera el intervalo de movimiento.
6. Se aplican los pasos al Modelo.
7. Al terminar se libera la orden.

**Resultado esperado**

```text
La unidad termina en B.
La UI no se bloquea.
La orden vuelve a null.
La unidad queda disponible.
El resultado del worker se publica.
```

**Evidencia automatizada**

- `MovimientoConcurrente_EjecutaEnWorkerYPublicaResultado`
- pruebas de `BuscadorRutaAStarTests`
- pruebas de `PlanificadorMovimientoTests`

**Estado:** **APROBADA — AUTOMATIZADA**

---

## PE-02 — Movimiento inválido

**Tipo:** Inválido

**Estado inicial**

Solicitud con ID inválido, unidad inexistente, destino inválido o movimiento no permitido.

**Pasos de escritorio**

1. Llega la solicitud.
2. Se validan ID, destino, disponibilidad y mapa.
3. La operación no debe alterar coordenadas.

**Resultado esperado**

```text
Exito = false
Modelo sin cambios
No queda una orden bloqueada
Mensaje explicativo
```

**Evidencia automatizada**

- `SolicitudInvalida_SeCompletaConResultadoLogicoRechazado`

**Estado:** **APROBADA — AUTOMATIZADA**

---

## PE-03 — Cancelación de movimiento

**Tipo:** Concurrente / cancelación

**Estado inicial**

Movimiento iniciado con un worker todavía pendiente.

**Pasos de escritorio**

1. Iniciar movimiento.
2. Antes de aplicar el paso, cancelar el proceso.
3. El `CancellationToken` es observado por el worker.

**Resultado esperado**

```text
Proceso = Cancelado
La posición no se aplica si todavía no correspondía
La unidad vuelve a quedar disponible
```

**Evidencia automatizada**

- `CancelarMovimiento_AntesDeAplicar_NoModificaModelo`
- `CancelarMovimiento_PorUnidad_CancelaWorkerYLiberaOrden`

**Estado:** **APROBADA — AUTOMATIZADA**

---

## PE-04 — Recolección completa con depósito

**Tipo:** Normal / concurrente

**Estado inicial**

```text
Aldeano disponible
Nodo de recurso disponible
Centro Urbano aliado existente
```

**Pasos de escritorio**

1. Seleccionar Aldeano.
2. Seleccionar recurso.
3. Aproximarse.
4. Extraer hasta llenar carga o agotar nodo.
5. Aproximarse al Centro Urbano.
6. Vaciar carga.
7. Agregar el recurso al saldo del jugador.

**Resultado esperado**

```text
El recurso físico disminuye.
El Aldeano transporta carga temporalmente.
Al depositar, la carga vuelve a 0.
El saldo del jugador aumenta.
Se registra RECOLECCION_DEPOSITO.
```

**Evidencia automatizada**

- `RecoleccionConcurrente_PublicaResultadoDesdeWorker`
- `CargaConservada_EnSiguienteOrdenSeDepositaAntesDeContinuar`
- `CicloRecoleccionTests`
- `DepositoRecoleccionTests`

**Estado:** **APROBADA — AUTOMATIZADA**

---

## PE-05 — Cancelar recolección

**Tipo:** Concurrente / cancelación

**Pasos de escritorio**

1. Iniciar recolección.
2. Cancelar antes del primer ciclo o después de una extracción parcial.
3. Comprobar estado de Aldeano y carga.

**Resultado esperado**

Si no se aplicó extracción:

```text
No se altera el recurso.
Proceso cancelado.
```

Si existe carga parcial:

```text
La carga no desaparece.
El Aldeano vuelve a Idle.
La siguiente orden puede depositar la carga conservada.
```

**Evidencia automatizada**

- `CancelarRecoleccion_AntesDeAplicar_DevuelveCancelado`
- `CancelarTrasPrimerCiclo_ConservaCargaParcialYVuelveIdle`
- `CargaConservada_EnSiguienteOrdenSeDepositaAntesDeContinuar`

**Estado:** **APROBADA — AUTOMATIZADA**

---

## PE-06 — Regeneración de recurso agotado

**Tipo:** Normal / concurrente

**Estado inicial**

Un nodo de Oro, Madera o Comida llega a cantidad 0.

**Pasos de escritorio**

1. `ServicioRegeneracionRecursos` detecta el nodo agotado.
2. Registra una regeneración pendiente.
3. Espera el retardo configurado.
4. Busca casillas libres.
5. Retira el nodo agotado.
6. Crea un nodo nuevo del mismo tipo en una casilla válida.

**Resultado esperado**

```text
Tipo nuevo = tipo agotado
Casilla nueva = libre y válida
No se superpone con edificios/unidades/recursos
Se registra RECURSO_REGENERADO
```

**Evidencia automatizada**

- `EjecutarPaso_RegeneraMismoTipoEnOtraCasilla`
- `Iniciar_EjecutaRegeneracionEnTaskConcurrente`

**Estado:** **APROBADA — AUTOMATIZADA**

---

## PE-07 — Construcción válida

**Tipo:** Normal / concurrente

**Estado inicial**

```text
Aldeano disponible
Saldo suficiente
Casilla válida
```

**Pasos de escritorio**

1. Validar costo y destino.
2. Reservar recursos/casilla.
3. Crear obra.
4. Avanzar progreso en worker.
5. Al llegar a 100%, crear edificio.
6. Retirar obra.

**Resultado esperado**

```text
Un solo edificio final.
Costo descontado correctamente.
Casilla ocupada.
Aldeano liberado al terminar.
```

**Evidencia automatizada**

- `ConstruccionConcurrente_CreaEdificioDesdeWorker`

**Estado:** **APROBADA — AUTOMATIZADA**

---

## PE-08 — Dos construcciones sobre la misma casilla

**Tipo:** Límite / concurrente / race condition

**Estado inicial**

Dos solicitudes intentan construir en la misma posición.

**Riesgo**

Sin sincronización, ambas podrían validar la casilla como libre y crear dos edificios.

**Resultado esperado**

```text
Solo una construcción obtiene la reserva.
La segunda es rechazada.
No se duplican edificios.
```

**Evidencia automatizada**

- `DosConstrucciones_MismaCasilla_SoloUnaSeAplica`
- `ConflictoConstruccion_Repetido_NoDuplicaEdificios`

**Estado:** **APROBADA — AUTOMATIZADA**

---

## PE-09 — Cancelación de construcción

**Tipo:** Concurrente / cancelación

**Pasos de escritorio**

1. Iniciar construcción.
2. Cancelarla antes de aplicar progreso definitivo.
3. Comprobar reserva y casilla.

**Resultado esperado**

```text
No aparece edificio terminado.
No queda una ocupación fantasma.
La operación limpia/reembolsa según el punto de cancelación.
```

**Evidencia automatizada**

- `CancelarConstruccion_AntesDeAplicar_NoOcupaCasilla`

**Estado:** **APROBADA — AUTOMATIZADA**

---

## PE-10 — Entrenamiento y cola

**Tipo:** Normal / concurrente

**Estado inicial**

Centro Urbano válido y recursos suficientes.

**Pasos de escritorio**

1. Solicitar dos entrenamientos.
2. Ambos entran a la cola.
3. Solo el primero progresa como cabeza de cola.
4. Al completar, se busca spawn.
5. Se crea la unidad.
6. El siguiente entrenamiento continúa.

**Resultado esperado**

```text
Ambas unidades aparecen en orden.
No comparten una casilla inválida.
El spawn es seguro.
La cola termina consistente.
```

**Evidencia automatizada**

- `EntrenamientoConcurrente_CreaUnidadDesdeWorker`
- `DosEntrenamientos_SeEncolanYAmbosUsanSpawnSeguro`

**Estado:** **APROBADA — AUTOMATIZADA**

---

## PE-11 — Gasto económico concurrente

**Tipo:** Concurrente / race condition

**Estado inicial**

```text
Saldo disponible = suficiente para una compra
Dos tareas intentan gastar el mismo costo simultáneamente
```

**Riesgo sin sincronización**

```text
Task A lee saldo suficiente
Task B lee saldo suficiente
Task A descuenta
Task B descuenta
=> saldo gastado dos veces
```

**Mecanismo**

`RecursosJugador.IntentarGastar(CostoRecursos)` ejecuta comprobación y descuento bajo un único `lock`.

**Resultado esperado**

```text
Solo una operación tiene éxito.
La otra falla por saldo insuficiente.
El saldo nunca queda negativo.
```

**Evidencia automatizada**

- `GastoCompuesto_EsAtomico`
- `DosGastosConcurrentes_ConSaldoParaUno_SoloUnoGana`

**Estado:** **APROBADA — AUTOMATIZADA**

---

## PE-12 — Operaciones distintas simultáneas

**Tipo:** Concurrente

**Estado inicial**

Una unidad se mueve mientras un Centro Urbano entrena.

**Pasos de escritorio**

1. Iniciar movimiento.
2. Sin esperar su finalización, iniciar entrenamiento.
3. Esperar ambos workers.

**Resultado esperado**

```text
Ambos procesos pueden coexistir.
Ninguno bloquea globalmente la partida.
Ambos publican su resultado.
```

**Evidencia automatizada**

- `MovimientoYEntrenamiento_PuedenEjecutarseSimultaneamente`

**Estado:** **APROBADA — AUTOMATIZADA**

---

## PE-13 — Ataque progresivo

**Tipo:** Normal / concurrente

**Estado inicial**

Atacante militar y objetivo enemigo vivo.

**Pasos de escritorio**

1. Validar enemigos.
2. Si está fuera de alcance, aproximar.
3. Iniciar worker de ataque.
4. Esperar intervalo.
5. Aplicar daño.
6. Repetir hasta destrucción o cancelación.

**Resultado esperado**

```text
Vida disminuye de forma progresiva.
El objetivo se elimina al llegar a 0.
La casilla queda liberada.
```

**Evidencia automatizada**

- `AtaqueConcurrente_ContinuaHastaDestruirObjetivo`
- `AtaqueConcurrente_FueraDeAlcance_SeAproximaYDestruyeObjetivo`

**Estado:** **APROBADA — AUTOMATIZADA**

---

## PE-14 — Cancelar ataque antes del impacto

**Tipo:** Concurrente / cancelación

**Pasos de escritorio**

1. Iniciar ataque con retardo.
2. Cancelar antes del primer impacto.

**Resultado esperado**

```text
Proceso cancelado.
Vida del objetivo sin modificación.
```

**Evidencia automatizada**

- `CancelarAtaque_AntesDeAplicar_NoReduceVida`

**Estado:** **APROBADA — AUTOMATIZADA**

---

## PE-15 — Defensa reactiva de IA

**Tipo:** Concurrente / IA

**Estado inicial**

Una IA ya puede tener su frente ofensivo normal. El Humano ataca otro Soldado de esa facción.

**Pasos de escritorio**

1. Aplicar un impacto humano al Soldado IA.
2. Publicar evento `ImpactoAplicado`.
3. Obtener el índice de la IA.
4. Intentar frente defensivo adicional.
5. Usar únicamente el ID de la unidad atacada.

**Resultado esperado**

```text
La unidad exacta atacada contraataca.
No se suman unidades vecinas.
El frente ofensivo normal puede continuar.
No se crean defensas duplicadas para la misma facción.
```

**Evidencia automatizada**

- `ImpactoHumano_CreaFrenteDefensivoExtra_SoloConUnidadAtacada`

**Estado:** **APROBADA — AUTOMATIZADA**

---

## PE-16 — Paseo idle exclusivo de IA

**Tipo:** Normal / IA

**Caso IA**

1. Aldeano IA libre.
2. Sin orden activa.
3. Existe casilla ortogonal libre.

Resultado:

```text
Puede desplazarse exactamente 1 casilla.
No crea OrdenActiva real.
Una orden prioritaria puede desplazar/cancelar el paseo.
```

**Caso Humano**

Aldeano humano sin tarea.

Resultado:

```text
Permanece quieto.
El HUD puede contabilizar su inactividad y avisar al jugador.
```

**Evidencia automatizada**

- `PlanificadorPaseo_AldeanoLibre_MueveSoloUnaCasillaOrtogonal`
- `MovimientoIdle_AldeanoIa_AvanzaSinCrearOrdenReal`
- pruebas actualizadas de `ReaccionAutomaticaTests` para Aldeanos humanos idle

**Estado:** **APROBADA — AUTOMATIZADA**

---

## PE-17 — Regla de victoria AND

**Tipo:** Límite / condición terminal

### Caso A

```text
Centro Urbano = existe
Militares = 0
```

Resultado esperado: la facción **no** queda eliminada.

### Caso B

```text
Centro Urbano = 0
Militares = existen
```

Resultado esperado: la facción **no** queda eliminada.

### Caso C

```text
Centro Urbano = 0
Militares = 0
```

Resultado esperado: la facción queda eliminada.

Para victoria humana deben quedar eliminadas las tres IAs.

**Evidencia automatizada**

- `ReglaAnd_SinMilitaresPeroConCentro_NoFinaliza`
- `ReglaAnd_SinCentroPeroConMilitar_NoFinaliza`
- `ReglaAnd_SinCentroYSinMilitares_DeclaraGanadorHumano`
- `ReglaAnd_PuedeDeclararGanadoraALaMaquina`

**Estado:** **APROBADA — AUTOMATIZADA**

---

## PE-18 — Finalización cancela workers

**Tipo:** Concurrente / límite

**Estado inicial**

Existe un worker de movimiento todavía en ejecución y durante ese tiempo se cumple la condición de victoria.

**Resultado esperado**

```text
Partida.Finalizada = true
Se registra ganador
Se genera resultado_final.txt
Nuevas órdenes son rechazadas
Worker pendiente termina Cancelado
```

**Evidencia automatizada**

- `EstadoPartida_Finaliza_GuardaResultadoYRechazaNuevasOrdenes`
- `Finalizacion_CancelaWorkerConcurrentePendiente`

**Estado:** **APROBADA — AUTOMATIZADA**

---

## PE-19 — Archivos obligatorios

**Tipo:** Normal / errores de IO

### configuracion.txt

Debe crearse al iniciar una partida con información inicial.

### log_partida.txt

Debe conservar eventos en orden.

### resultado_final.txt

Debe conservar:

- ganador;
- perdedores;
- motivo;
- estado final del mapa;
- recursos;
- saldos;
- edificios;
- unidades;
- obras.

**Casos inválidos**

Contenido nulo debe rechazarse. Un problema de permisos debe manejarse sin corromper la partida.

**Evidencia automatizada**

- `GuardarConfiguracionInicial_IncluyeAmbosJugadoresYSobrescribe`
- `RegistrarEvento_CreaArchivoYConservaEventosEnOrden`
- `GuardarResultadoPartidaFinalizada_IncluyeEstadoFinalDelMapa`
- pruebas de acceso denegado en `RegistroAccionesTests`

**Estado:** **APROBADA — AUTOMATIZADA**

---

## PE-20 — Networking WebSocket opcional

**Tipo:** Integración / concurrente

El networking no es requisito obligatorio del alcance final, pero permanece como evidencia técnica.

**Pasos de escritorio**

1. Recibir JSON.
2. Deserializar `MensajeRedPartida`.
3. Validar tipo.
4. Despachar a `ServicioAccionesConcurrentes`.
5. Publicar respuesta.

**Mensajes cubiertos**

```text
MOVER
RECOLECTAR
CONSTRUIR
ENTRENAR
ATACAR
CURAR
```

**Resultado esperado**

Un mensaje válido usa las mismas reglas del gameplay. Un mensaje inválido se rechaza sin derribar el servicio.

**Evidencia automatizada**

- `MensajeMover_DespachaWorkerYModificaModelo`
- `MensajeRecolectar_UsaMismasReglasConcurrentes`
- `MensajeConstruir_DespachaConstruccion`
- `MensajeEntrenar_DespachaEntrenamiento`
- `MensajeAtacar_DespachaIntencionDeAtaque`
- `MensajeCurar_DespachaCuracionConcurrente`
- `MensajeInvalido_SeRechazaSinLanzarExcepcion`

**Estado:** **APROBADA — AUTOMATIZADA / EXTRA**

---

# 3. Pruebas manuales finales pendientes

Las siguientes pruebas no deben marcarse como completadas hasta ejecutarlas visualmente sobre la versión final.

| ID | Prueba manual | Resultado esperado | Estado |
|---|---|---|---|
| PM-01 | Abrir menú principal | Menú visible y funcional | PENDIENTE |
| PM-02 | Abrir/cerrar instrucciones | Panel legible y retorna al menú | PENDIENTE |
| PM-03 | Iniciar partida | Mapa, Humano y 3 IAs aparecen correctamente | PENDIENTE |
| PM-04 | Aviso Aldeano humano inactivo | Aldeano humano queda quieto y HUD avisa | PENDIENTE |
| PM-05 | Aldeano IA idle | Puede hacer paseo corto sin afectar economía | PENDIENTE |
| PM-06 | Recolección visual | Recurso muestra feedback visual durante extracción | PENDIENTE |
| PM-07 | Construcción visual | Obra progresa hasta edificio | PENDIENTE |
| PM-08 | Entrenamiento visual | Unidad aparece tras progreso | PENDIENTE |
| PM-09 | Combate visual | Vida cambia; <=25% se tiñe rojo | PENDIENTE |
| PM-10 | Pausa | Partida deja de avanzar y puede reanudarse | PENDIENTE |
| PM-11 | Victoria/derrota | Pantalla final bloquea interacción | PENDIENTE |
| PM-12 | Build Windows/Linux | Ejecutable abre correctamente | PENDIENTE |
| PM-13 | Launcher | Inicia API + juego y cierra API al salir | PENDIENTE |
| PM-14 | Archivos reales | Se generan los tres .txt con contenido de una partida real | PENDIENTE |

# 4. Conclusión

La suite automatizada final cubre los casos críticos exigidos por la guía: normales, inválidos, límite y concurrentes.

El riesgo técnico principal de concurrencia se aborda mediante:

- `lock`;
- `ConcurrentDictionary`;
- `ConcurrentQueue`;
- `Interlocked`;
- `SemaphoreSlim`;
- `CancellationToken`.

La validación automatizada actual es:

```text
390 / 390 pruebas correctas
```

El cierre definitivo requiere completar la tabla de pruebas manuales finales y conservar evidencia del build y de los archivos generados.
