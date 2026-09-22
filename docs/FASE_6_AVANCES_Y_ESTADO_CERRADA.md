# FASE 6 — COMBATE, VICTORIA Y ESTABILIZACIÓN FINAL

**Proyecto:** Imperios en Guerra  
**Modalidad:** Humano vs Máquina  
**Rama:** `feature/fase6-combate-victoria`  
**Estado técnico:** TERMINADA Y VALIDADA  
**Integración:** PR #75 listo para merge hacia `develop`  
**Validación final supervisada:** suite completa local con 0 errores

---

## 1. Objetivo

Cerrar el ciclo completo de la partida:

```text
inicio
→ economía
→ movimiento
→ construcción/entrenamiento
→ combate
→ daño
→ destrucción
→ condición terminal
→ ganador
→ resultado_final.txt
```

Se mantuvo MVC y la regla arquitectónica del proyecto: el Modelo contiene estado/reglas, los servicios/controladores coordinan y Unity representa la Vista.

---

## 2. Decisión explícita sobre estadísticas de combate

La guía disponible exige ataques, destrucción y condición de victoria, pero no define valores numéricos de:

- vida;
- daño;
- armadura;
- alcance;
- intervalos de ataque.

Después del smoke test se comprobó que la regla inicial de “un impacto = destrucción” hacía el combate demasiado agresivo.

Por decisión explícita del equipo se definió un **balance propio del prototipo**, centralizado en `ConfiguracionCombate`. Estos valores no se presentan como requisitos del profesor ni como valores oficiales de Age of Empires.

| Entidad | Vida | Daño | Alcance | Intervalo |
|---|---:|---:|---:|---:|
| Aldeano | 60 | 0 | 0 | — |
| Guerrero | 120 | 30 | 1 | 4 s |
| Lancero | 100 | 25 | 1 | 3.5 s |
| Arquero | 80 | 20 | 3 | 3 s |
| Monje | 70 | 15 | 2 | 5 s |
| Centro Urbano | 300 | — | — | — |

Las subclases técnicas usadas por pruebas reciben un perfil neutral no ofensivo para no convertir infraestructura de testing en entidades de balance real.

---

## 3. Combate real

`Unidad` y `Edificio` conservan vida sincronizada. El acceso/modificación de vida se protege con `lock`.

`OperacionAtaque` valida:

- partida existente y no finalizada;
- atacante existente;
- atacante militar ofensivo;
- objetivo enemigo;
- mapa lógico compartido;
- objetivo existente como Unidad o Edificio;
- alcance real según tipo de atacante.

El alcance usa distancia Chebyshev:

```text
max(|dx|, |dy|)
```

Por tanto una diagonal inmediata cuenta como una casilla. El movimiento/pathfinding continúa ortogonal y no fue sustituido.

Flujo:

```text
ataque
→ aplicar daño
→ VidaActual disminuye
→ si VidaActual > 0, objetivo continúa
→ si VidaActual = 0, eliminar entidad
→ liberar casilla
→ evaluar victoria
```

---

## 4. Aproximación automática para atacar

Se añadió `PlanificadorAproximacionAtaque` en el Modelo.

Cuando el usuario selecciona **Atacar** y el objetivo está lejos:

1. se localizan casillas libres dentro del alcance del atacante;
2. se calcula una ruta válida usando el pathfinding existente;
3. la unidad se mueve progresivamente hasta la posición de ataque;
4. la orden cambia de `Mover` a `Atacar`;
5. se espera el intervalo configurado;
6. se aplica un impacto.

La unidad nunca intenta ocupar la casilla del Centro Urbano.

Unity sincroniza el snapshot durante el proceso para mostrar `Moviendo` y luego `Atacando`.

---

## 5. Identidad y objetivos

`Edificio` posee `Guid Id` estable.

El mismo flujo de ataque puede identificar:

- unidades enemigas;
- Centros Urbanos enemigos.

La API y los DTO de Unity transportan IDs, vida y datos de combate.

---

## 6. Máquina y combate

La Máquina utiliza los mismos servicios y validaciones que el Humano.

Ajustes realizados durante Fase 6:

- no persigue Aldeanos como objetivo militar prioritario;
- prioriza unidades militares humanas;
- utiliza el alcance real de su atacante;
- puede aproximarse antes de atacar;
- intenta formar una fuerza militar antes del asalto a base;
- mantiene **un único frente de combate activo** para evitar bajas simultáneas excesivas;
- economía, recolección, construcción y entrenamiento continúan concurrentes.

---

## 7. Regla de victoria AND

La guía usa la expresión “Centro Urbano y/o unidades militares”. El equipo fijó explícitamente la semántica terminal como:

**AND**

```text
sin Centros Urbanos
Y
sin unidades militares
= derrota
```

Por lo tanto:

- sin militares pero con Centro Urbano → continúa;
- sin Centro Urbano pero con militares → continúa;
- sin ambos → derrota.

`Partida` conserva:

- `Finalizada`;
- `Ganador`;
- `MotivoFinalizacion`.

La victoria solo se evalúa después de una destrucción real, no después de un impacto que deje vida restante.

---

## 8. Finalización y concurrencia

`EstadoPartidaService` publica un evento de finalización.

Cuando se cumple la regla terminal:

- se rechazan nuevas órdenes;
- `ServicioAccionesConcurrentes` cancela workers activos;
- `ServicioJugadorMaquina` detiene su ciclo;
- la cancelación sigue siendo cooperativa mediante `CancellationToken`;
- se escribe el resultado final;
- Unity bloquea interacción.

También se estabilizó una race condition entre finalización de partida y un worker de movimiento ya iniciado.

---

## 9. resultado_final.txt

`ServicioArchivos` sigue centralizando `System.IO`.

Se generan:

- `configuracion.txt`;
- `log_partida.txt`;
- `resultado_final.txt`.

`resultado_final.txt` incluye:

```text
RESULTADO_FINAL
Estado=Finalizada
ReglaVictoria=AND
GanadorTipo=...
GanadorNombre=...
PerdedorTipo=...
PerdedorNombre=...
Motivo=...
```

Los errores de IO se manejan sin dispersar acceso a archivos dentro de MonoBehaviours.

---

## 10. Vista y HUD

El snapshot expone:

- estado de partida;
- ganador/motivo;
- vida actual/máxima;
- daño;
- alcance;
- IDs de unidades/edificios.

Unity:

- oculta entidades destruidas;
- muestra vida de entidades;
- muestra daño/alcance de combatientes;
- muestra estados `Moviendo` y `Atacando`;
- tiñe de rojo las entidades con **25% de vida o menos**;
- bloquea clics/selección al finalizar;
- muestra una pantalla completa de VICTORIA/DERROTA;
- deja únicamente el botón **SALIR** en la pantalla final.

El feedback de vida crítica es puramente visual y no modifica reglas.

---

## 11. Spawn y economía visibles

El spawn de entrenamiento se estabilizó:

- evita bordes si existe alternativa interior;
- prioriza posiciones con más salidas ortogonales;
- libera la marca temporal de ocupación al abandonar el spawn;
- evita obstáculos fantasma.

La escena de prueba contiene:

```text
Oro:    4 nodos
Madera: 4 nodos
Comida: 6 nodos
Total: 14 nodos físicos
```

El selector de entrenamiento muestra costos compactos por unidad:

```text
O = Oro
M = Madera
C = Comida
```

---

## 12. Networking y ataque

Unity usa el endpoint concurrente de ataque:

```text
POST /api/partida/atacar-concurrente
```

y consulta:

```text
GET /api/procesos/{id}/resultado
```

Esto corrigió el 404 detectado en smoke test y mantiene ataque dentro del mismo patrón concurrente usado por el resto de acciones.

WebSocket + JSON continúa soportando:

- MOVER;
- RECOLECTAR;
- CONSTRUIR;
- ENTRENAR;
- ATACAR.

---

## 13. Pruebas y correcciones

Durante Fase 6 se cubrieron, entre otros:

- impacto con vida restante;
- destrucción al llegar a 0 HP;
- Guerrero melee;
- Arquero a distancia;
- ataque diagonal;
- aproximación automática;
- ataque a Centro Urbano;
- ataque concurrente;
- cancelación de ataque;
- IA sin persecución prioritaria de Aldeanos;
- un solo frente militar de IA;
- regla AND parcial/completa;
- ganador Humano/Máquina;
- archivo final;
- rechazo de órdenes tras finalizar;
- cancelación global;
- acceso a recursos;
- spawn seguro;
- networking;
- feedback visual crítico;
- bloqueo de interacción final.

Históricamente Fase 6 pasó por varias estabilizaciones. El cierre definitivo fue validado localmente por el equipo con la **suite completa en verde y 0 errores**.

Las advertencias intencionales de acceso denegado a `log_partida.txt` pertenecen a una prueba de manejo de errores de IO.

---

## 14. Commits principales de Fase 6

```text
90ffe44 feat: agregar destrucción de entidades y evaluación de victoria
3bfa3ea test: adaptar regresiones al ataque adyacente
6be14b9 feat: cerrar victoria AND y resultado final
be2b245 fix: estabilizar cancelación global al finalizar partida
1ca44d2 fix: mejorar acceso a recursos y compactar HUD
3580af2 fix: corregir ataque Unity y agresividad de la maquina
1280abc fix: serializar ofensiva de la maquina
cabe6d8 fix: distinguir partida finalizada de desconexion API
5a6181a feat: agregar vida daño y alcance al combate
46e31ea fix: permitir alcance de combate en diagonal
42fd391 feat: aproximar ataques y mostrar estado en vivo
2e2b48d fix: evitar spawns atrapados y mejorar economia visible
db4e08a feat: agregar vida critica y pantalla final bloqueante
```

---

## 15. Estado de cierre

```text
FASE 0      TERMINADA
FASE 1      TERMINADA
FASE 2      TERMINADA
FASE 3      TERMINADA
FASE 4      TERMINADA
ETAPA 4.5   TERMINADA
FASE 5      TERMINADA / DEVELOP
FASE 6      TERMINADA Y VALIDADA / PR #75
```

No quedan nuevas mecánicas pendientes dentro de Fase 6.

Siguiente paso de proceso:

```text
PR #75
→ merge a develop
→ verificar develop
→ etapa final de entrega/documentación general
```
