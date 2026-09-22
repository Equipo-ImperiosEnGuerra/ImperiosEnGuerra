# FASE 6 — COMBATE, VICTORIA Y ARCHIVOS FINALES

**Proyecto:** Imperios en Guerra  
**Modalidad:** Humano vs Máquina  
**Rama:** `feature/fase6-combate-victoria`  
**Estado técnico:** TERMINADO Y VALIDADO EN .NET  
**Integración:** pendiente de Pull Request hacia `develop`  
**Validación final supervisada:** 320/320 pruebas correctas

---

## 1. Objetivo

Cerrar el ciclo completo de la partida:

```text
inicio
→ economía
→ movimiento
→ construcción/entrenamiento
→ combate
→ destrucción
→ condición terminal
→ ganador
→ resultado_final.txt
```

Se mantuvo MVC y la regla arquitectónica del proyecto: el Modelo contiene reglas/estado, los servicios/controladores coordinan y Unity representa la Vista.

---

## 2. Análisis previo de combate

La documentación disponible exige ataques, destrucción y condición de victoria, pero no fija valores numéricos para:

- puntos de vida;
- daño;
- armadura;
- alcance específico por unidad;
- bonificaciones entre tipos de tropa.

Por esa razón no se introdujeron números arbitrarios.

La regla actual del prototipo es:

```text
ataque válido
+ objetivo enemigo ortogonalmente adyacente
= impacto destructivo
```

Esta decisión mantiene el combate funcional sin presentar estadísticas inventadas como requisitos del profesor.

---

## 3. Identidad de edificios

`Edificio` recibió un `Guid Id` estable.

Esto permite que el mismo contrato de ataque pueda identificar:

- unidades;
- Centros Urbanos.

El ID de edificios se expone por API y llega a Unity para selección/ataque.

---

## 4. Ataque real

`OperacionAtaque` ahora valida:

- partida existente/no finalizada;
- atacante existente;
- atacante disponible o con orden de ataque activa;
- atacante militar;
- objetivo enemigo;
- mapa lógico compartido;
- objetivo existente como Unidad o Edificio;
- adyacencia ortogonal.

Cuando el impacto es válido:

1. la entidad objetivo se elimina de su jugador;
2. se libera su casilla del mapa;
3. se evalúa el estado de victoria;
4. Unity puede ocultar la entidad ausente en el siguiente snapshot.

Los workers de ataque usan la misma infraestructura concurrente y una orden activa por unidad.

---

## 5. Regla de victoria AND

La guía utiliza la expresión “Centro Urbano y/o unidades militares”. Como esa semántica no estaba fijada, el equipo tomó una decisión explícita antes de programarla:

**AND**

Un jugador pierde solamente cuando se cumplen simultáneamente:

```text
no queda ningún Centro Urbano
Y
no queda ninguna unidad militar
```

Por lo tanto:

- sin militares pero con Centro Urbano → la partida continúa;
- sin Centro Urbano pero con militares → la partida continúa;
- sin ambos → derrota.

`Partida` conserva:

- `Finalizada`;
- `Ganador`;
- `MotivoFinalizacion`.

---

## 6. Finalización y concurrencia

`EstadoPartidaService` publica un evento de finalización.

Cuando se alcanza la condición terminal:

- `ServicioAccionesConcurrentes` solicita la cancelación de todos los workers activos;
- `ServicioJugadorMaquina` detiene su ciclo de decisiones;
- nuevas órdenes se rechazan;
- la cancelación sigue siendo cooperativa mediante `CancellationToken`.

Se añadió una prueba específica para evitar una race condition entre finalización y un worker de movimiento ya activo.

---

## 7. resultado_final.txt

`ServicioArchivos` sigue siendo el único responsable de la escritura de archivos del juego.

Al finalizar, genera `resultado_final.txt` con estructura similar a:

```text
RESULTADO_FINAL
Estado=Finalizada
ReglaVictoria=AND
GanadorTipo=Humano
GanadorNombre=Humano
PerdedorTipo=Maquina
PerdedorNombre=CPU
Motivo=...
```

Se conserva además la generación previa de:

- `configuracion.txt`;
- `log_partida.txt`.

Los errores de IO se manejan sin dispersar `System.IO` por MonoBehaviours.

---

## 8. API y Unity

El snapshot de partida expone:

- estado activa/finalizada;
- tipo de ganador;
- nombre del ganador;
- motivo;
- IDs de edificios.

Unity:

- elimina visualmente unidades/edificios que ya no existen en el snapshot;
- permite seleccionar edificio enemigo como objetivo;
- bloquea nuevas acciones al recibir estado final;
- muestra VICTORIA si gana el Humano;
- muestra DERROTA si gana la Máquina.

La lógica terminal sigue estando en C#; el HUD únicamente representa el resultado recibido.

---

## 9. Pruebas

Baselines de Fase 6:

```text
314/314
319/320 durante estabilización de cancelación
320/320 final
```

El resultado final supervisado fue:

```text
Total: 320
Correctas: 320
Errores: 0
Omitidas: 0
```

Las pruebas nuevas/revisadas cubren:

- ataque adyacente válido;
- objetivo lejano inválido;
- ataque Humano → Máquina;
- ataque Máquina → Humano;
- destrucción de unidad;
- destrucción de Centro Urbano;
- liberación de casilla;
- objetivo propio inválido;
- IDs inválidos;
- ataque concurrente;
- cancelación de ataque;
- regla AND parcial;
- regla AND completa;
- ganador Humano;
- ganador Máquina;
- generación de `resultado_final.txt`;
- rechazo de órdenes tras finalizar;
- cancelación global de worker pendiente;
- regresiones de networking/IA/concurrencia.

---

## 10. Race condition corregida

Durante la prueba final apareció una carrera entre:

```text
worker de movimiento iniciándose
vs.
partida finalizando
```

Se corrigió haciendo que el worker vuelva a observar su `CancellationToken` después de planificar y antes de convertir un fallo de inicio de orden en resultado normal.

La prueba también espera a que la orden de movimiento esté realmente activa antes de provocar el final, por lo que valida una cancelación real y no el scheduling del ThreadPool.

---

## 11. Commits principales

```text
90ffe44 feat: agregar destrucción de entidades y evaluación de victoria
3bfa3ea test: adaptar regresiones al ataque adyacente
6be14b9 feat: cerrar victoria AND y resultado final
ff0dccc fix: resolver ambigüedad al guardar resultado final
be2b245 fix: estabilizar cancelación global al finalizar partida
```

---

## 12. Estado de cierre

```text
FASE 0      TERMINADA
FASE 1      TERMINADA
FASE 2      TERMINADA
FASE 3      TERMINADA
FASE 4      TERMINADA
ETAPA 4.5   TERMINADA
FASE 5      TERMINADA / DEVELOP
FASE 6      TERMINADA TÉCNICAMENTE / FEATURE
```

Siguiente paso:

```text
Pull Request
→ revisión
→ merge a develop
→ estabilización/entrega/documentación final
```

La suite .NET está verde. Antes de la entrega final sigue siendo recomendable un smoke test visual completo en Unity de victoria/derrota, destrucción y bloqueo de controles.
