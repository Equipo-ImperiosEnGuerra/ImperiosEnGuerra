# Auditoría final contra la guía del proyecto

**Proyecto:** Imperios en Guerra  
**Referencia principal:** Guía de Proyecto: Implementación del Juego "Age of Empires" en C# y Unity  
**Rama de auditoría:** `docs/auditoria-final-guia`  
**Base:** `main` sincronizado después del PR #122

## Criterios de estado

- **CUMPLE:** existe implementación/evidencia directa en el repositorio.
- **CUMPLE / VALIDAR:** la implementación existe, pero falta una prueba final de entrega o evidencia consolidada.
- **PENDIENTE DOCUMENTAR:** el código existe, pero falta el entregable formal exigido por la guía.
- **NO APLICA — ALCANCE ACTUALIZADO:** aparece en la guía original, pero dejó de ser requisito para la entrega según la aclaración posterior del docente comunicada al equipo.

## Matriz de cumplimiento

| Requisito de la guía | Estado | Evidencia actual | Acción final |
|---|---|---|---|
| C# + Unity, build de escritorio | CUMPLE | Build Windows generado y ejecutado correctamente; launcher inicia API + juego y detiene la API al cerrar; HUD standalone validado | Mantener evidencia final |
| Código orientado a objetos | CUMPLE | Jerarquías `Unidad`, `Soldado`, `Edificio`; clases `Jugador`, `Mapa`, `Partida`, recursos y operaciones; `docs/UML_FINAL.md` | Ninguna |
| MVC claro | CUMPLE | `src/ImperiosEnGuerra.Modelo`, `Assets/Scripts/Vistas`, `Assets/Scripts/Controladores`; Modelo sin UnityEngine; explicado en `docs/INFORME_TECNICO_FINAL.md` | Ninguna |
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
| Anuncio de ganador | CUMPLE | Pantalla final implementada; DERROTA validada visualmente y condición de VICTORIA cubierta por tests automatizados | Evidencia suficiente para cierre |
| `configuracion.txt` | CUMPLE | Validado con partida real: mapa 15x15, Humano + 3 IAs, bases/unidades iniciales y 26 recursos físicos | Evidencia obtenida |
| `log_partida.txt` | CUMPLE | `ServicioArchivos.RegistrarEvento`; funcionamiento validado con eventos reales y archivo limpio regenerado sobre la versión final de `main` | Evidencia obtenida |
| `resultado_final.txt` | CUMPLE | Validado con una derrota real: ganador, perdedor, motivo, mapa final 15x15, recursos restantes, saldos, edificios, unidades, vida y obras | Evidencia obtenida |
| System.IO | CUMPLE | `ServicioArchivos` centralizado | Ninguna |
| Manejo de excepciones | CUMPLE / VALIDAR | Validaciones, try/catch y pruebas de errores de IO/red | Consolidar evidencia |
| Colecciones | CUMPLE | List, Dictionary y colecciones concurrentes en Modelo/Servicios | Ninguna |
| Mensajes claros al usuario | CUMPLE | HUD, mensajes de error, progreso, victoria/derrota, menú/instrucciones | Revisar visual final |
| Cuidado visual real | CUMPLE / VALIDAR | Tiny Swords, HUD, sprites, menú y feedback visual | Capturas/build final |
| Pruebas normales, inválidas, límite y concurrentes | CUMPLE / VALIDAR | Suite .NET ejecutada sobre `main`: 390 correctas, 0 errores, 0 omitidas; existen además pruebas Unity EditMode | Ejecutar validación final de Unity y smoke test del build |
| Pruebas de networking | CUMPLE a nivel unitario/integración | `NetworkingTests.cs` | Añadir evidencia de prueba manual si aplica |
| Pruebas de ataques | CUMPLE | múltiples suites de ataque y concurrencia | Consolidar resultados |
| Pruebas de victoria | CUMPLE | `VictoriaFinalTests.cs` y pruebas visuales | Consolidar resultados |
| Informe breve MVC/concurrencia/red | CUMPLE | `docs/INFORME_TECNICO_FINAL.md` documenta MVC, POO, concurrencia, sincronización, archivos, excepciones y networking opcional | Revisión final de consistencia |
| Diagrama de clases UML | CUMPLE | `docs/UML_FINAL.md` separa explícitamente MODELO, CONTROLADOR/APLICACIÓN, VISTA UNITY y servicios transversales, y además detalla herencia/composición del dominio | Ninguna |
| Diagrama de flujo | CUMPLE | `docs/DIAGRAMA_FLUJO_FINAL.md` documenta flujo general, acciones concurrentes, recolección, construcción, entrenamiento, combate, IA y pausa | Revisión final de consistencia |
| Pruebas de escritorio formales | CUMPLE / VALIDAR | `docs/PRUEBAS_ESCRITORIO_FINAL.md` formaliza casos normales, inválidos, límite y concurrentes; las pruebas manuales finales siguen pendientes | Completar evidencia manual y build |
| Documentación formal | CUMPLE / VALIDAR EVIDENCIA | README, auditoría, informe técnico, UML, diagramas de flujo, pruebas de escritorio y checklist de validación final consolidados | Conservar evidencias finales pendientes indicadas en el checklist |

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

### 4. Validación .NET y archivo final

Después de integrar el estado final del mapa en `resultado_final.txt` y corregir el hotfix de compilación, se ejecutó la suite sobre `main`:

```text
Total: 390
Correctas: 390
Errores: 0
Omitidas: 0
```

Las dos advertencias de acceso denegado a `log_partida.txt` corresponden a una prueba intencional de manejo de errores de IO.

El archivo `resultado_final.txt` ahora conserva la información anterior de ganador/perdedores y agrega una instantánea lógica del estado final del mapa: dimensiones, recursos físicos restantes, saldos por jugador, edificios, unidades y obras en curso.

### 5. README actualizado

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
3. ~~Ejecutar suite .NET desde `main`.~~ **COMPLETADO: 390/390 correctas, 0 errores.**
4. ~~Ejecutar Unity EditMode/PlayMode y smoke test del build siguiendo `docs/VALIDACION_FINAL_ENTREGA.md`.~~ **BUILD + LAUNCHER VALIDADOS.**
5. ~~Guardar ejemplos reales de los tres archivos obligatorios después de una partida final.~~ **COMPLETADO.**
6. ~~Crear informe final MVC + concurrencia + networking opcional.~~ **COMPLETADO**
7. ~~Crear UML basado en el código final.~~ **COMPLETADO**
8. ~~Crear diagrama de flujo.~~ **COMPLETADO**
9. ~~Crear documento de pruebas de escritorio.~~ **COMPLETADO; faltan evidencias manuales indicadas dentro del documento.**
10. ~~Hacer revisión final de consistencia entre documentación y código.~~ **COMPLETADO para el PR documental; quedan únicamente evidencias manuales señaladas en el checklist.**
