# Auditoría final contra la guía del proyecto

**Proyecto:** Imperios en Guerra  
**Referencia principal:** Guía de Proyecto: Implementación del Juego "Age of Empires" en C# y Unity  
**Base funcional:** versión estable integrada en `main` antes de la auditoría de comentarios  
**Rama documental actual:** `docs/comentarios-bloque1-modelo-base`  
**Estado de esta auditoría:** código y documentación revisados; quedan revalidación del candidato final y empaquetado de evidencias.

> La guía original exige dos jugadores conectados. La modalidad final Humano vs 3 Máquinas y el carácter opcional del networking provienen de una aclaración posterior comunicada al equipo y **no aparecen en el PDF original**. Debe conservarse una evidencia externa de esa aclaración para la sustentación.

## Criterios de estado

- **CUMPLE:** existe implementación y evidencia verificable en el repositorio.
- **CUMPLE FUNCIONAL / EVIDENCIA PENDIENTE:** la funcionalidad existe y fue validada, pero falta incorporar una copia de evidencia al paquete final.
- **PENDIENTE VALIDACIÓN FINAL:** existe implementación, pero debe repetirse la prueba sobre el commit candidato definitivo.
- **NO APLICA — ALCANCE ACTUALIZADO:** requisito de la guía original sustituido posteriormente según la aclaración del docente comunicada al equipo.

## Matriz de cumplimiento

| Requisito de la guía | Estado | Evidencia actual | Acción final |
|---|---|---|---|
| C# + Unity, build de escritorio | CUMPLE | Unity 6000.6.0f1, build Windows y launcher implementados | Repetir smoke test sobre candidato final |
| Código orientado a objetos | CUMPLE | Jerarquías `Unidad`, `Soldado`, `Edificio`; clases `Jugador`, `Mapa`, `Partida` y operaciones | Ninguna |
| MVC claro | CUMPLE | `src/ImperiosEnGuerra.Modelo`, `Assets/Scripts/Vistas`, `Assets/Scripts/Controladores`; Modelo sin UnityEngine | Ninguna |
| Mapa lógico y representación gráfica | CUMPLE | `Mapa`, `Casilla`, `VistaPartida` y escena Unity | Conservar captura final |
| Oro, Madera y Comida | CUMPLE | `TipoRecurso`, `Recurso`, `RecursosJugador` y HUD | Ninguna |
| Centro Urbano inicial | CUMPLE | `CentroUrbano` e `InicializadorPartida` | Ninguna |
| Validar límites y superposición | CUMPLE | Modelo de mapa, ocupación, pathfinding, spawn seguro y tests | Ninguna |
| Construcción | CUMPLE | Construcción progresiva, reserva de costo/casilla, aproximación, progreso, cancelación y reembolso | Conservar evidencia visual |
| Entrenamiento | CUMPLE | Cola, costo, progreso y spawn seguro | Conservar evidencia visual |
| Movimiento | CUMPLE | A*, movimiento progresivo y worker concurrente | Conservar evidencia visual |
| Ataque | CUMPLE | Vida, daño, alcance, aproximación, destrucción y ataque concurrente | Conservar evidencia visual |
| Recolección continua con hilos/Task | CUMPLE | `ServicioAccionesConcurrentes.IniciarRecoleccion` + `GestorProcesosConcurrentes` | Explicarlo en sustentación |
| Construcción con hilos/Task | CUMPLE | `IniciarConstruccion` mediante gestor concurrente | Explicar cancelación/reembolso |
| Entrenamiento con hilos/Task | CUMPLE | `IniciarEntrenamiento` mediante gestor concurrente | Explicar cola y cancelación |
| Movimiento con hilos/Task | CUMPLE | `IniciarMovimiento` mediante gestor concurrente | Explicar Main Thread |
| Sincronización de datos compartidos | CUMPLE | `lock`, `ConcurrentDictionary`, `ConcurrentQueue`, `Interlocked`, `SemaphoreSlim`, `CancellationToken` | Preparar ejemplos de race conditions |
| Workers no modifican UnityEngine | CUMPLE | Servicios concurrentes separados + test `ServiciosConcurrencia_NoReferenciaUnityEngine` | Explicarlo en sustentación |
| Listener de red no bloqueante | CUMPLE COMO EXTRA | `ServicioRedPartida.AtenderClienteAsync` + WebSocket asíncrono | Conservar como evidencia técnica |
| Mensajes de red estructurados JSON | CUMPLE COMO EXTRA | `MensajeRedPartida`, `System.Text.Json`, despachador y tests | Conservar como evidencia técnica |
| Red: construir, entrenar, mover, atacar | CUMPLE COMO EXTRA | `NetworkingTests` cubre MOVER, RECOLECTAR, CONSTRUIR, ENTRENAR, ATACAR y CURAR | No requiere segundo cliente para alcance actualizado |
| Manejo de desconexiones/errores de red | CUMPLE COMO EXTRA | Cierre, cancelación, límites de mensaje y rechazo de JSON inválido | Ninguna |
| Dos jugadores / dos instancias | NO APLICA — ALCANCE ACTUALIZADO | Exigido en guía original; sustituido por Humano vs 3 Máquinas según aclaración posterior | Conservar evidencia de la aclaración docente |
| Comunicación de acciones a la aplicación del oponente | NO APLICA — ALCANCE ACTUALIZADO | WebSocket se conserva como extensión; no hay segundo cliente Unity obligatorio | Conservar evidencia de la aclaración docente |
| Condición de victoria | CUMPLE con decisión de diseño | `EvaluadorVictoria`; regla AND: sin Centros Urbanos y sin militares | Explicar que la guía usa “y/o” |
| Anuncio de ganador | CUMPLE | Pantalla final implementada y bloqueo de interacción | Revalidar victoria visual en candidato final |
| `configuracion.txt` | CUMPLE FUNCIONAL / EVIDENCIA PENDIENTE | Se genera y fue validado manualmente; `DatosPartida/` está ignorado por Git | Copiar un ejemplo real al paquete de entrega |
| `log_partida.txt` | CUMPLE FUNCIONAL / EVIDENCIA PENDIENTE | Se genera y fue validado manualmente; `DatosPartida/` está ignorado por Git | Copiar un ejemplo limpio al paquete de entrega |
| `resultado_final.txt` | CUMPLE FUNCIONAL / EVIDENCIA PENDIENTE | Se genera y fue validado manualmente; `DatosPartida/` está ignorado por Git | Copiar un ejemplo real al paquete de entrega |
| System.IO | CUMPLE | `ServicioArchivos` centraliza escritura y formatos | Ninguna |
| Manejo de excepciones | CUMPLE | Validaciones, try/catch y pruebas de IO/red | Ninguna |
| Colecciones | CUMPLE | `List<T>`, `Dictionary<TKey,TValue>` y colecciones concurrentes | Ninguna |
| Mensajes claros al usuario | CUMPLE | HUD, mensajes de error, progreso, menús y pantalla final | Smoke test final |
| Cuidado visual real | CUMPLE FUNCIONAL / EVIDENCIA PENDIENTE | Tiny Swords, HUD, sprites, menús, feedback de vida y animación de recursos | Incorporar capturas al paquete final |
| Pruebas normales, inválidas, límite y concurrentes | PENDIENTE VALIDACIÓN FINAL | Última ejecución funcional previa: 390 correctas, 0 errores, 0 omitidas; tests Unity EditMode existentes | Reejecutar sobre candidato final |
| Pruebas de networking | CUMPLE | `NetworkingTests.cs` | Ninguna |
| Pruebas de ataques | CUMPLE | `AtaqueTests` y `AtaqueConcurrenteTests` | Ninguna |
| Pruebas de victoria | CUMPLE | `VictoriaFinalTests.cs` | Revalidar pantalla VICTORIA manualmente |
| Informe MVC/concurrencia/red | CUMPLE | `docs/INFORME_TECNICO_FINAL.md` | Ninguna |
| Diagrama de clases UML | CUMPLE | `docs/UML_FINAL.md`, corregido contra firmas reales | Ninguna |
| Diagrama de flujo | CUMPLE | `docs/DIAGRAMA_FLUJO_FINAL.md`, alineado con construcción, combate y logging reales | Ninguna |
| Pruebas de escritorio formales | CUMPLE | `docs/PRUEBAS_ESCRITORIO_FINAL.md` con trazabilidad a tests concretos | Completar evidencia manual final |
| Documentación formal | CUMPLE | README, auditoría, informe, UML, flujo, pruebas y checklist | Integrar esta rama y congelar versión final |

## Hallazgos principales

### 1. Alcance original vs alcance final

El PDF original exige dos jugadores, dos instancias y comunicación entre ambos. La modalidad final del proyecto es:

```text
1 jugador Humano
vs
3 facciones controladas por Máquina
(Morada, Verde y Amarilla)
```

Este cambio **no está documentado en el PDF original**. La documentación del repositorio lo trata como alcance actualizado porque fue comunicado posteriormente al equipo. Para evitar una discusión durante la sustentación, debe conservarse la captura, mensaje, correo o evidencia donde el docente aceptó ese cambio.

### 2. Networking conservado como extensión técnica

Aunque ya no se usa como requisito de dos clientes Unity, el repositorio conserva:

- endpoint WebSocket;
- escucha asíncrona;
- conexiones en `ConcurrentDictionary`;
- JSON;
- mensajes MOVER, RECOLECTAR, CONSTRUIR, ENTRENAR, ATACAR y CURAR;
- `SemaphoreSlim` para serializar envíos por conexión;
- despacho hacia `ServicioAccionesConcurrentes`;
- pruebas de mensajes válidos e inválidos.

### 3. Condición “y/o”

La guía utiliza “Centro Urbano y/o unidades”. El proyecto adopta de forma explícita:

```text
sin Centros Urbanos
Y
sin unidades militares
= facción eliminada
```

La decisión está centralizada en `EvaluadorVictoria` y cubierta por tests.

### 4. Evidencias generadas no versionadas

`.gitignore` excluye:

```text
/src/ImperiosEnGuerra.Api/DatosPartida/
```

Por eso los tres archivos obligatorios pueden generarse correctamente y, aun así, no aparecer en el repositorio. Antes de entregar debe conservarse una **copia de evidencia fuera de esa carpeta ignorada**, por ejemplo dentro del paquete de entrega o una carpeta de evidencias expresamente versionada.

Tampoco se encontraron capturas versionadas dentro de `docs/`. Las validaciones visuales descritas en los documentos se consideran antecedentes manuales, pero no sustituyen el paquete final de evidencias.

### 5. Estado real de la suite después de los comentarios

La referencia funcional conocida es:

```text
Total: 390
Correctas: 390
Errores: 0
Omitidas: 0
```

La rama actual añadió comentarios C# y correcciones documentales. No se afirma que esa misma cifra haya sido reejecutada después de dichos cambios. Aunque no se modificó la lógica, el candidato final debe compilar y volver a ejecutar la suite antes del cierre.

### 6. UML y flujo auditados contra código

Se corrigieron dos puntos documentales concretos:

- `ControladorAcciones.PrepararAccion` es privado, por lo que en UML se representa con `-`.
- `DespachadorMensajesRed` expone `Procesar(string)`, no `DespacharAsync(...)`.

También se aclaró que `ControladorConexionApi` no referencia directamente a `EstadoPartidaService`: la comunicación pasa por HTTP/JSON y los endpoints de `Program.cs`.

El diagrama de flujo de construcción ahora incluye la aproximación del Aldeano antes del progreso de obra, y el flujo de victoria separa correctamente eliminación de IA y derrota del Humano.

## Orden de cierre recomendado

1. Reejecutar `dotnet test` sobre el commit candidato final.
2. Abrir Unity y confirmar que la rama/merge compila sin errores.
3. Ejecutar el smoke test completo del build.
4. Validar visualmente una victoria Humana final.
5. Copiar `configuracion.txt`, `log_partida.txt` y `resultado_final.txt` al paquete de evidencias.
6. Guardar capturas de menú, partida, recolección, construcción, entrenamiento, combate, vida crítica, pausa y victoria.
7. Conservar evidencia del cambio de alcance autorizado por el docente.
8. Revisar el PR de esta auditoría y mergear.
9. Repetir el smoke test sobre `main` si el proceso de integración cambia cualquier archivo funcional.
10. Congelar la entrega.
