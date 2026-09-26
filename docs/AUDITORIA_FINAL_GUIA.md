# Auditoría final contra la guía del proyecto

**Proyecto:** Imperios en Guerra  
**Referencia principal:** Guía de Proyecto: Implementación del Juego "Age of Empires" en C# y Unity  
**Rama de auditoría:** `docs/auditoria-final-guia`  
**Base:** `main` después del PR #113

## Criterios de estado

- **CUMPLE:** existe implementación/evidencia directa en el repositorio.
- **CUMPLE / VALIDAR:** la implementación existe, pero falta una prueba final de entrega o evidencia consolidada.
- **PENDIENTE DOCUMENTAR:** el código existe, pero falta el entregable formal exigido por la guía.
- **REQUIERE ACLARACIÓN DOCENTE:** la versión actual difiere del texto original de la guía y debe documentarse la modificación posterior de alcance.

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
| Listener de red no bloqueante | CUMPLE en servidor | `ServicioRedPartida.AtenderClienteAsync` + WebSocket asíncrono | Ver punto de dos instancias |
| Mensajes de red estructurados JSON | CUMPLE | `MensajeRedPartida`, `System.Text.Json`, despachador y tests | Ninguna |
| Red: construir, entrenar, mover, atacar | CUMPLE a nivel de protocolo/dispatcher | `NetworkingTests` cubre MOVER, RECOLECTAR, CONSTRUIR, ENTRENAR, ATACAR y CURAR | Verificar requisito de dos instancias reales |
| Manejo de desconexiones/errores de red | CUMPLE / VALIDAR | Servicio WebSocket maneja cierre, cancelación y errores; mensajes inválidos se rechazan | Hacer prueba demostrable de desconexión |
| Dos jugadores / dos instancias | REQUIERE ACLARACIÓN DOCENTE | El texto original lo exige; la versión actual es Humano vs 3 Máquinas | Documentar formalmente la modificación de alcance comunicada posteriormente por el profesor |
| Comunicación de acciones a la aplicación del oponente | REQUIERE ACLARACIÓN DOCENTE | Existe servidor WebSocket + protocolo, pero no se evidencia un cliente WebSocket Unity para dos ejecutables de jugador | Si networking entre jugadores dejó de ser obligatorio, registrar la aclaración; si sigue vigente, implementar/demo de dos clientes |
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

### 1. Cambio de modalidad respecto a la guía original

La guía original describe dos jugadores y una instancia por jugador. El proyecto actual funciona como un Humano contra tres facciones de Máquina.

El equipo indicó durante el desarrollo que el profesor cambió la modalidad a Humano vs Máquina y posteriormente aclaró que WebSockets era opcional. Esta modificación debe quedar escrita en la documentación final o acompañada de la evidencia disponible de la aclaración para que el evaluador no compare la versión actual como si siguiera vigente el requisito original de dos jugadores humanos.

### 2. Networking: implementación técnica vs escenario original

El repositorio sí contiene:

- endpoint WebSocket;
- servicio de escucha asíncrono;
- conexiones en `ConcurrentDictionary`;
- serialización JSON;
- mensajes para MOVER, RECOLECTAR, CONSTRUIR, ENTRENAR, ATACAR y CURAR;
- despacho hacia las mismas operaciones concurrentes del gameplay;
- tests de mensajes y errores.

Sin embargo, no aparece un cliente `ClientWebSocket` dentro de Unity que conecte dos ejecutables de jugadores entre sí. Por tanto, si se auditara literalmente la guía original, la demostración de “dos instancias del juego” quedaría incompleta. Si la aclaración posterior del profesor eliminó ese requisito, no es una tarea de implementación pendiente: es una tarea de documentación.

### 3. Condición “y/o”

La guía usa “Centro Urbano y/o unidades”. El proyecto fijó la condición final como AND:

```text
sin Centros Urbanos
Y
sin unidades militares
= derrota
```

La regla está implementada y probada, pero la documentación final debe aclarar que se tomó una decisión concreta sobre una formulación ambigua de la guía.

### 4. README desactualizado

El README todavía presenta Fase 6 como “lista para merge a develop”, aunque la versión ya llegó a `main` después de Fase 7. Debe actualizarse.

También debe revisarse la tabla de Monje: el Modelo actual usa su capacidad como curación (15, alcance 2, intervalo 5 s), mientras `ConfiguracionCombate` le asigna daño de ataque 0. El README no debe presentarlo como daño ofensivo.

## Orden de cierre recomendado

1. Actualizar `README.md` al estado final real.
2. Documentar la modificación de alcance Humano vs Máquina / networking opcional.
3. Ejecutar suite .NET desde `main`.
4. Ejecutar Unity EditMode/PlayMode y smoke test del build.
5. Guardar ejemplos de los tres archivos obligatorios.
6. Crear informe final MVC + concurrencia + networking.
7. Crear UML basado en el código final.
8. Crear diagrama de flujo.
9. Crear documento de pruebas de escritorio.
10. Hacer revisión final de consistencia entre documentación y código.
