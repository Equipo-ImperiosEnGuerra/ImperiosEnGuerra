# FASE 3 — AVANCES Y ESTADO ACTUAL

## 1. Estado general

```text
PROYECTO: IMPERIOS EN GUERRA
FASE 3 — CONTROLADORES, INTERACCIÓN Y ACCIONES
ESTADO: EN DESARROLLO
```

La Fase 3 convierte la Vista inicial en una interfaz interactiva sin trasladar las reglas del juego a Unity.

Regla principal vigente:

```text
Unity = entrada, HUD y representación
Controladores Unity = adaptadores de interacción
API = puente
C# fuera de Unity = reglas, validaciones y cambios de estado
```

Los `MonoBehaviour` no deben convertirse en el cerebro del juego.

---

## 2. Punto de partida

La Fase 3 comenzó después de cerrar:

- Fase 0;
- Fase 1;
- Fase 1.5;
- Fase 2 completa.

La arquitectura recibida fue:

```text
Unity
  ↓ HTTP/JSON
ImperiosEnGuerra.Api
  ↓
ImperiosEnGuerra.Modelo
  └─ ImperiosEnGuerra.Servicios
```

Al comenzar Fase 3 ya existían:

- `VistaPartida`;
- DTOs de estado;
- mapa visual;
- recursos visuales;
- edificios Humano/Máquina;
- soporte visual de unidades;
- `ControladorConexionApi`;
- API de inicio/consulta de partida;
- 102 pruebas automatizadas en verde.

---

## 3. Plan de Fase 3

La fase se estructuró en bloques:

```text
BLOQUE A
C-00 → C-03
Auditoría + selección

BLOQUE B
C-04 → C-07
HUD + preparación de acciones

BLOQUE C
C-08 → C-11
Identidad + movimiento + recolección base

BLOQUE D
C-12 → C-18
Construcción + entrenamiento + ataque

BLOQUE E
C-19 → C-24
Refresco + logs + pruebas + cierre
```

---

# BLOQUE A — SELECCIÓN

## 4. C-00 — Auditoría previa

**Estado: TERMINADO**

Antes de modificar código se revisaron:

- `VistaPartida`;
- `ControladorConexionApi`;
- DTOs Unity;
- Modelo;
- API;
- escena;
- Input System;
- cámara;
- posibilidad de usar `Physics2D`;
- forma de identificar entidades visuales.

Hallazgos principales:

- los GameObjects visuales no tenían metadatos estructurados;
- no tenían `Collider2D`;
- no existía selección;
- unidad/edificio/recurso no tenían ID estable;
- tipo + propietario + coordenada era suficiente solo para selección visual temporal, no para gameplay real;
- la Vista reconstruye su contenido al volver a renderizar.

Decisión:

> implementar selección visual sin introducir IDs todavía y resolver la identidad estable antes del movimiento real.

---

## 5. C-01 — Entidades seleccionables

**Estado: TERMINADO**

Se creó:

```text
Assets/Scripts/Vistas/EntidadSeleccionableVista.cs
```

Responsabilidades:

- almacenar categoría visual;
- tipo lógico;
- propietario;
- coordenada lógica X/Y;
- gestionar resaltado visual;
- conservar/restaurar color original.

No contiene reglas de gameplay.

`VistaPartida` agrega este componente a:

- recursos;
- edificios;
- unidades.

También se agregaron `BoxCollider2D` para permitir detección por clic.

---

## 6. C-02 — Selección por clic

**Estado: TERMINADO**

Se creó:

```text
Assets/Scripts/Controladores/ControladorSeleccion.cs
```

Utiliza el Input System nuevo y `Physics2D.OverlapPointAll`.

Flujo:

```text
clic
  ↓
posición pantalla
  ↓
posición mundo
  ↓
Physics2D
  ↓
EntidadSeleccionableVista
  ↓
selección actual
```

La selección resuelve superposiciones por orden visual (`sortingOrder`) y un desempate determinista.

Comportamiento validado:

- seleccionar recurso;
- seleccionar Centro Urbano Humano;
- seleccionar Centro Urbano Máquina;
- cambiar selección;
- clic vacío limpia selección;
- Escape limpia selección.

Seleccionar una entidad enemiga no concede control sobre ella.

---

## 7. C-03 — Resaltado visual

**Estado: TERMINADO**

El resaltado pertenece a `EntidadSeleccionableVista`, no al Controlador.

Flujo:

```text
ControladorSeleccion
  ↓ ordena
MostrarSeleccion / OcultarSeleccion
  ↓
Vista decide cómo representarlo
```

También se incorporó limpieza de selección antes de que `VistaPartida` reconstruya su contenido para evitar referencias inválidas.

Se añadió conversión inversa preparada para acciones posteriores:

```text
posición mundo
  ↓
TryObtenerCoordenadaLogica
  ↓
(x, y) lógico válido
```

No hace clamp de puntos fuera del mapa.

---

## 8. Validación Bloque A

Resultado:

- selección visual comprobada en Unity;
- recursos y edificios Humano/Máquina seleccionables;
- resaltado funcionando;
- clic vacío/Escape funcionando;
- Unity sin warnings/errores funcionales;
- 102/102 pruebas continuaron en verde;
- `git diff --check` limpio antes de integrar.

Git:

- PR #23 — `feat: implementar seleccion visual de entidades`.

---

# BLOQUE B — HUD Y PREPARACIÓN DE ACCIONES

## 9. C-04 — HUD básico

**Estado: TERMINADO**

Se creó:

```text
Assets/Scripts/Vistas/VistaHud.cs
```

El HUD muestra:

```text
Oro
Madera
Comida
```

para el jugador humano.

También muestra:

- entidad seleccionada;
- propietario;
- coordenada;
- mensajes de información/acción.

Los recursos llegan mediante:

```text
ControladorConexionApi
  ↓ EstadoPartidaDto
VistaHud
```

`VistaHud` no consulta la API directamente.

---

## 10. C-05 — Panel contextual

**Estado: TERMINADO**

Las acciones visibles dependen de la selección actual.

Comportamiento definido:

| Selección | Acciones de interfaz |
|---|---|
| Recurso | Ninguna |
| Edificio Humano | Entrenar |
| Edificio Máquina | Ninguna acción propia |
| Aldeano Humano | Mover, Recolectar, Construir |
| Guerrero/Lancero/Arquero/Monje Humano | Mover, Atacar |
| Unidad Máquina | Ninguna acción propia |

Estas acciones todavía representan intención, no ejecución real.

El HUD se configuró con:

- Canvas `Screen Space - Overlay`;
- `CanvasScaler`;
- `GraphicRaycaster`;
- `EventSystem` compatible con Input System;
- barra superior de recursos;
- panel contextual compacto inferior izquierdo.

Se creó:

```text
Assets/Editor/Fase3HudConfigurator.cs
```

Menú:

```text
Tools
→ Imperios en Guerra
→ Fase 3
→ Configurar HUD y acciones
```

El configurador es idempotente.

---

## 11. C-06 — Contratos de acciones

**Estado: TERMINADO**

Se crearon POCO en el Modelo:

```text
src/ImperiosEnGuerra.Modelo/Acciones/
├── TipoAccionJuego.cs
├── SolicitudAccion.cs
└── ResultadoAccion.cs
```

`TipoAccionJuego` prepara las intenciones:

- Mover;
- Recolectar;
- Construir;
- Entrenar;
- Atacar.

`SolicitudAccion` se mantuvo mínima para no forzar campos prematuramente.

No se agregaron reglas de gameplay a Unity.

---

## 12. C-07 — Resultado estándar

**Estado: TERMINADO**

`ResultadoAccion` permite representar de forma uniforme:

- éxito;
- fallo;
- mensaje.

Ejemplo conceptual:

```text
ResultadoAccion.Exitoso("Acción aceptada")
ResultadoAccion.Fallido("Destino inválido")
```

Esto se utilizará cuando las acciones reales empiecen a pasar por API/Modelo.

---

## 13. ControladorAcciones

Se creó:

```text
Assets/Scripts/Controladores/ControladorAcciones.cs
```

Responsabilidad actual:

- recibir clics de botones del HUD;
- traducirlos a intención;
- validar únicamente coherencia de interfaz/selección;
- mostrar que una acción quedó preparada.

Ejemplo validado:

```text
Centro Urbano Humano seleccionado
  ↓
Entrenar
  ↓
"Intención Entrenar preparada. Ejecución pendiente de una fase posterior."
```

No crea unidades ni modifica recursos.

---

## 14. Regla definitiva sobre controladores

Durante Fase 3 se aclaró la separación:

### Controladores/adaptadores dentro de Unity

Pueden:

- detectar input;
- usar cámara;
- usar raycasts/Physics2D;
- leer botones;
- llamar la API;
- actualizar Vistas;
- mostrar feedback.

Ejemplos:

- `ControladorSeleccion`;
- `ControladorAcciones`;
- `ControladorConexionApi`.

### Lógica fuera de Unity

Debe contener:

- reglas de movimiento;
- validaciones de recursos;
- reglas de construcción;
- entrenamiento;
- ataque;
- cambios reales de estado.

Regla resumida:

```text
MonoBehaviour puede capturar intención.
MonoBehaviour NO debe decidir reglas del dominio.
```

---

## 15. Validación Bloque B

Al cerrar el Bloque B:

```text
111/111 pruebas NUnit correctas
```

Se validó en Unity:

- HUD visible;
- Oro/Madera/Comida visibles;
- selección actual reflejada en HUD;
- coordenada visible;
- panel contextual;
- `Entrenar` visible para Centro Urbano Humano;
- acción de Entrenar solo prepara intención;
- entidades enemigas no reciben acciones propias;
- Console sin errores funcionales.

Git:

- PR #24 — `feat: agregar HUD y preparacion de acciones`.

---

## 16. Mejora visual Tiny Swords anotada

Se revisaron los elementos UI disponibles en Tiny Swords y se decidió **no incorporarlos todavía** para no retrasar gameplay.

Pendiente para una fase de pulido:

```text
Icon_02 → Madera
Icon_03 → Oro
Icon_04 → Comida

Botones Tiny Swords
Banners
Ribbons
Wood Table / Papers
Cursores contextuales
Barras de vida/progreso
Animaciones de recolección/construcción/combate
Efectos y partículas
```

La mejora visual queda explícitamente subordinada a estabilidad y requisitos técnicos.

---

## 17. Estado real C-00 a C-24

| ID | Tarea | Estado actual |
|---|---|---|
| C-00 | Auditoría | **TERMINADO** |
| C-01 | Entidades seleccionables | **TERMINADO** |
| C-02 | Selección por clic | **TERMINADO** |
| C-03 | Resaltado visual | **TERMINADO** |
| C-04 | HUD básico | **TERMINADO** |
| C-05 | Panel contextual | **TERMINADO** |
| C-06 | Contratos de acciones | **TERMINADO** |
| C-07 | Resultado estándar | **TERMINADO** |
| C-08 | Identidad estable de unidades | **SIGUIENTE / PENDIENTE** |
| C-09 | Seleccionar unidad + destino | PENDIENTE |
| C-10 | Movimiento lógico base | PENDIENTE |
| C-11 | Preparar flujo de recolección | PENDIENTE |
| C-12/C-16 | Construcción y entrenamiento | PENDIENTE |
| C-17/C-20 | Ataque, refresco y errores | PENDIENTE |
| C-21/C-24 | Logging, pruebas integradas y cierre | PENDIENTE |

---

## 18. Issues de continuidad

Para permitir que otro integrante continúe correctamente se crearon Issues en GitHub.

Pendientes principales:

```text
#25 — C-08 identidad estable de unidades
#26 — C-09/C-10 movimiento lógico
#27 — C-11 recolección base
#28 — C-12/C-16 construcción y entrenamiento
#29 — C-17/C-20 ataque, refresco y errores
#30 — C-21/C-24 cierre Fase 3
#31 — Fase 4 concurrencia/sincronización
#32 — Fase 5 Máquina + revisión networking
#33 — Fase 6 combate/victoria/archivos
#34 — Fase 7 integración/documentación/pulido
```

También se crearon Issues históricos cerrados para dejar trazabilidad de las fases y bloques ya terminados.

---

## 19. Punto exacto de continuación

La próxima tarea lógica es:

> **C-08 — IDENTIDAD ESTABLE DE UNIDADES**

Problema actual:

```text
tipo + propietario + coordenada
```

sirve para selección visual temporal, pero no garantiza identidad estable porque la unidad puede moverse y puede haber varias unidades del mismo tipo.

Flujo objetivo:

```text
Unidad.Id
  ↓
API
  ↓
DTO Unity
  ↓
VistaPartida
  ↓
EntidadSeleccionableVista
  ↓
ControladorSeleccion
```

La identidad debe:

- originarse fuera de Unity;
- ser estable;
- ser única dentro de la partida;
- no cambiar al moverse;
- poder viajar por JSON/API;
- no depender del nombre del GameObject ni del índice de una lista.

Solo después se implementará movimiento real.

---

## 20. Flujo objetivo de movimiento

Después de C-08:

```text
seleccionar unidad humana
        ↓
Mover
        ↓
clic en destino
        ↓
unidadId + coordenada
        ↓
API
        ↓
C# puro valida
        ↓
Modelo actualiza
        ↓
API devuelve estado
        ↓
Unity refresca
```

Validaciones deberán ocurrir fuera de Unity:

- unidad existente;
- propiedad Humano;
- disponibilidad;
- límites del mapa;
- ocupación;
- destino válido.

La concurrencia temporal del movimiento se reserva para Fase 4.

---

## 21. Qué NO está implementado todavía

A día de este documento todavía faltan:

- ID estable de unidades;
- movimiento real;
- recolección real;
- construcción real;
- entrenamiento real;
- ataque real;
- puntos de vida/daño si no están definidos;
- condición de victoria completa;
- IA funcional de la Máquina;
- concurrencia real en gameplay;
- sincronización de datos compartidos;
- `CancellationToken` aplicado;
- cola/evento thread-safe hacia Unity;
- networking revisado/implementado para la modalidad vigente;
- `log_partida.txt` conectado a todas las acciones reales;
- `resultado_final.txt` conectado al fin de partida;
- documentación final/UML final;
- pulido visual Tiny Swords.

---

## 22. Fases posteriores

Roadmap vigente:

```text
FASE 3 — Gameplay base                 EN DESARROLLO
        ↓
FASE 4 — Concurrencia y sincronización PENDIENTE
        ↓
FASE 5 — Máquina + networking revisado PENDIENTE
        ↓
FASE 6 — Combate, victoria y archivos  PENDIENTE
        ↓
FASE 7 — Integración y documentación   PENDIENTE
```

---

## 23. Reglas que siguen vigentes

- guía del profesor tiene prioridad;
- no inventar requisitos;
- cambio mínimo sobre código existente;
- MVC claro;
- Modelo sin `UnityEngine`;
- Unity principalmente Vista/entrada;
- controladores Unity delgados;
- reglas reales fuera de `MonoBehaviour`;
- concurrencia real de C# en Fase 4;
- Coroutines no sustituyen `Thread`/`Task`;
- workers no tocan `UnityEngine` directamente;
- sincronización explícita;
- networking debe revisarse y no eliminarse silenciosamente;
- `System.IO` centralizado;
- pruebas antes de integrar;
- Issue → branch → desarrollo → pruebas → commit → PR → `develop`.

---

## 24. Estado resumido para recuperación de contexto

```text
PROYECTO: IMPERIOS EN GUERRA

Fase 0       TERMINADO
Fase 1       TERMINADO
Fase 1.5     TERMINADO
Fase 2       TERMINADO

Fase 3       EN DESARROLLO
C-00/C-07    TERMINADOS
C-08         SIGUIENTE

Pruebas al último bloque integrado:
111/111

Modalidad:
HUMANO VS MÁQUINA

Arquitectura:
Unity -> API -> Modelo / Servicios

Siguiente trabajo:
IDENTIDAD ESTABLE DE UNIDADES
```

---

## 25. Instrucción para la próxima sesión o integrante

No reiniciar Fase 3.

Primero:

1. comprobar `develop` actualizado y limpio;
2. abrir el Issue #25;
3. revisar `Unidad`, DTOs, mapper y selección;
4. implementar únicamente identidad estable;
5. probar;
6. integrar por PR;
7. continuar con Issue #26.

No implementar movimiento, concurrencia, IA o combate antes de cerrar las dependencias correspondientes.

---

## Fuentes de referencia del estado

Este documento consolida el código integrado, PR #23 y #24, las pruebas ejecutadas, las validaciones manuales en Unity y los Issues de continuidad creados en GitHub.

Debe actualizarse al cerrar nuevos bloques de Fase 3, manteniendo el código real como fuente principal.