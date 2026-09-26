# Auditoría final contra la guía del proyecto

**Proyecto:** Imperios en Guerra  
**Referencia principal:** Guía de Proyecto: Implementación del Juego "Age of Empires" en C# y Unity  
**Rama de auditoría:** `docs/auditoria-final-guia`  
**Base:** `main` después del PR #113

## Criterios de estado

- **CUMPLE:** existe implementación/evidencia directa en el repositorio.
- **CUMPLE / VALIDAR:** la implementación existe, pero falta una prueba final de entrega o evidencia consolidada.
- **PENDIENTE DOCUMENTAR:** el código existe, pero falta el entregable formal exigido por la guía.
- **NO APLICA — ALCANCE ACTUALIZADO:** aparece en la guía original, pero dejó de ser requisito para la entrega según la aclaración posterior del docente comunicada al equipo.

## Matriz de cumplimiento

| Requisito de la guía | Estado | Evidencia actual | Acción final |
|---|---|---|---|
| C# + Unity, build de escritorio | CUMPLE / VALIDAR | Proyecto Unity, scripts C#, `BuildStandaloneImperios.cs`, scripts de ejecución y `docs/BUILD_Y_EJECUCION.md` | Ejecutar smoke test final del build de `main` |
| Código orientado a objetos | CUMPLE | Jerarquías `Unidad`, `Soldado`, `Edificio`; clases `Jugador`, `Mapa`, `Partida`, recursos y operaciones | Documentar clases fundamentales en UML |
| MVC claro | CUMPLE | `src/ImperiosEnGuerra.Modelo`, `Assets/Scripts/Vistas`, `Assets/Scripts/Controladores`; Modelo sin UnityEngine | Explicar responsabilidades y flujo en informe |
| Mapa lógico y representación gráfica | CUMPLE | `Mapa`, `Casilla`, `VistaPartida`, escena Unity | Evidencia visual final |
| Oro, Madera y Comida | CUMPLE | `TipoRecurso`, `Recurso`, `RecursosJugador`, HUD | Ninguna |
| Centro Urbano inicial | CUMPLE | `CentroUrbano`, `InicializadorPartida` | Ninguna |
| Validar límites y superposición | CUMPLE | Modelo de mapa, ocupación, pathfinding, spawn seguro y pruebas | Incluir casos en pruebas de escritorio |
| Construcción | CUMPLE | Construcción progresiva concurrente, costos, reserva y reembolso | Evidencia de ejecución |
| Entrenamiento | CUMPLE | Cola, progreso, costo y spawn seguro | Evidencia de ejecución |
| Movimiento | CUMPLE | A*, movimiento progresivo y worker concurrente | Evidencia de ejecución |
| Ataque | CUMPLE | Vida, daño, rango, aproximación, destrucción y ataque concurrente | Evidencia de ejecución |
| Recolección continua con hilos/Task | CUMPLE | `ServicioAccionesConcurrentes.IniciarRecoleccion` + `GestorProcesosConcurrentes` | Explicar worker y sincronización |
| Construcción con hilos/Task | CUMPLE | `IniciarConstruccion` ejecutado mediante gestor concurrente | Explicar cancelación |
| Entrenamiento con hilos/Task | CUMPLE | `IniciarEntrenamiento` ejecutado mediante gestor concurrente | Explicar cola y sincronización |
| Movimiento con hilos/Task | CUMPLE | `IniciarMovimiento` ejecutado mediante gestor concurrente | Explicar Main Thread |
| Sincronización de datos compartidos | CUMPLE | `lock`, `ConcurrentDictionary`, `ConcurrentQueue`, `Interlocked`, `SemaphoreSlim`, `CancellationToken` | Preparar ejemplos de race conditions |
| Workers no modifican UnityEngine | CUMPLE | Servicio concurrente separado y prueba `ServiciosConcurrencia_NoReferenciaUnityEngine` | Explicarlo en sustentación |
| Listener de red no bloqueante | CUMPLE COMO EXTRA | `ServicioRedPartida.AtenderClienteAsync` + WebSocket asíncrono | Conservar como evidencia técnica opcional |
| Mensajes de red estructurados JSON | CUMPLE COMO EXTRA | `MensajeRedPartida`, `System.Text.Json`, despachador y tests | Conservar como evidencia técnica opcional |
| Red: construir, entrenar, mover, atacar | CUMPLE COMO EXTRA | `NetworkingTests` cubre MOVER, RECOLECTAR, CONSTRUIR, ENTRENAR, ATACAR y CURAR | No requiere dos ejecutables para el alcance final |
| Manejo de desconexiones/errores de red | CUMPLE COMO EXTRA | Servicio WebSocket maneja cierre, cancelación y errores; mensajes inválidos se rechazan | No es criterio obligatorio del alcance final |
| Dos jugadores / dos instancias | NO APLICA — ALCANCE ACTUALIZADO | La guía original lo exigía; el alcance final aceptado es 1 Humano vs 3 Máquinas | Ninguna implementación adicional |
| Comunicación de acciones a la aplicación del oponente | NO APLICA — ALCANCE ACTUALIZADO | El networking entre dos jugadores dejó de ser obligatorio; el WebSocket existente se conserva como extensión | Ninguna implementación adicional |
| Condición de victoria | CUMPLE con decisión de diseño | `EvaluadorVictoria` y regla actual AND: sin Centros Urbanos y sin militares | Documentar que la guía dice “y/o” y justificar la regla final aprobada |
| Anuncio de ganador | CUMPLE | HUD/pantalla final bloqueante | Evidencia visual |
| `configuracion.txt` | CUMPLE | `ServicioArchivos.GuardarConfiguracionInicial` | Adjuntar ejemplo generado |
| `log_partida.txt` | CUMPLE | `ServicioArchivos.RegistrarEvento` | Adjuntar ejemplo generado |
| `resultado_final.txt` | CUMPLE | `GuardarResultadoPartidaFinalizada` | Adjuntar ejemplo generado |
| System.IO | CUMPLE | `ServicioArchivos` centralizado | Ninguna |
| Manejo de excepciones | CUMPLE / VALIDAR | Validaciones, try/catch y pruebas de errores de IO/red | Consolidar evidencia |
| Colecciones | CUMPLE | List, Dictionary y colecciones concurrentes en Modelo/Servicios | Ninguna |
| Mensajes claros al usuario | CUMPLE | HUD, mensajes de error, progreso, victoria/derrota, menú/instrucciones | Revisar visual final |
| Cuidado visual real | CUMPLE / VALIDAR | Tiny Swords, HUD, sprites, menú y feedback visual | Capturas/build final |
| Pruebas normales, inválidas, límite y concurrentes | CUMPLE / VALIDAR | Suite extensa .NET + Unity EditMode | Reejecutar suite desde `main` después de los últimos merges |
| Pruebas de networking | CUMPLE a nivel unitario/integración | `NetworkingTests.cs` | Añadir evidencia de prueba manual si aplica |
| Pruebas de ataques | CUMPLE | múltiples suites de ataque y concurrencia | Consolidar resultados |
| Pruebas de victoria | CUMPLE | `VictoriaFinalTests.cs` y pruebas visuales | Consolidar resultados |
| Informe breve MVC/concurrencia/red | PENDIENTE DOCUMENTAR | README y docs por fase contienen material parcial | Crear informe formal final |
| Diagrama de clases UML | PENDIENTE DOCUMENTAR | No existe un UML final en el árbol actual | Crear UML basado en código real |
| Diagrama de flujo | PENDIENTE DOCUMENTAR | No existe diagrama final en el árbol actual | Crear flujo general y/o de acciones concurrentes |
| Pruebas de escritorio formales | PENDIENTE DOCUMENTAR | Existen tests automatizados, pero falta documento formal de evidencia | Crear matriz/casos/resultados |
| Documentación formal | PENDIENTE DOCUMENTAR | Existen README y documentos por fase | Consolidar entrega final |

## Hallazgos importantes

### 1. Alcance final respecto a la guía original

La guía original describe dos jugadores y una instancia por jugador. Ese requisito fue reemplazado posteriormente para la entrega: **ya no es necesario implementar dos jugadores ni dos instancias conectadas**.

La modalidad final del proyecto es:

```text
1 jugador Humano
vs
3 facciones controladas por Máquina
(Morada, Verde y Amarilla)
```

Por tanto, la ausencia de un segundo cliente Unity **no se considera un incumplimiento del alcance final**. La guía original se conserva como referencia histórica de requisitos, pero este punto queda marcado como **NO APLICA — ALCANCE ACTUALIZADO**.

### 2. Networking opcional conservado como extensión

El networking dejó de ser obligatorio para la entrega final. Aun así, el repositorio conserva una implementación funcional como evidencia técnica adicional:

- endpoint WebSocket;
- servicio de escucha asíncrono;
- conexiones en `ConcurrentDictionary`;
- serialización JSON;
- mensajes para MOVER, RECOLECTAR, CONSTRUIR, ENTRENAR, ATACAR y CURAR;
- despacho hacia las mismas operaciones concurrentes del gameplay;
- tests de mensajes y errores.

No se requiere implementar un cliente `ClientWebSocket` en un segundo ejecutable Unity. El componente de red existente puede explicarse en la sustentación como **extra técnico** y como ejemplo adicional de concurrencia, JSON, sincronización y manejo de conexiones.

### 3. Condición “y/o”

La guía usa “Centro Urbano y/o unidades”. El proyecto fijó la condición final como AND:

```text
sin Centros Urbanos
Y
sin unidades militares
= derrota
```

La regla está implementada y probada, pero la documentación final debe aclarar que se tomó una decisión concreta sobre una formulación ambigua de la guía.

### 4. README actualizado

El README fue actualizado durante esta auditoría para:

- reflejar Fase 7 integrada en `main`;
- registrar el alcance final Humano vs 3 Máquinas;
- indicar que dos instancias/networking entre jugadores ya no son obligatorios;
- conservar WebSocket + JSON como extensión técnica;
- documentar la estrategia diferenciada de Morada, Verde y Amarilla;
- corregir al Monje: **0 daño ofensivo** y curación de 15 a alcance 2 cada 5 s.

## Orden de cierre recomendado

1. ~~Actualizar `README.md` al estado final real.~~ **COMPLETADO**
2. ~~Documentar la modificación de alcance Humano vs Máquina / networking opcional.~~ **COMPLETADO**
3. Ejecutar suite .NET desde `main`.
4. Ejecutar Unity EditMode/PlayMode y smoke test del build.
5. Guardar ejemplos de los tres archivos obligatorios.
6. Crear informe final MVC + concurrencia + networking opcional.
7. Crear UML basado en el código final.
8. Crear diagrama de flujo.
9. Crear documento de pruebas de escritorio.
10. Hacer revisión final de consistencia entre documentación y código.
