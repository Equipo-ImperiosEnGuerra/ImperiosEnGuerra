# FASE 2 — AVANCES Y ESTADO CERRADA

## 1. Estado general

```text
PROYECTO: IMPERIOS EN GUERRA
FASE 2 — VISTA INICIAL EN UNITY
ESTADO: TERMINADO
```

La Fase 2 tuvo como objetivo representar visualmente en Unity el estado lógico del juego sin trasladar las reglas del dominio a `MonoBehaviour`.

Al cierre de la fase se mantiene la regla arquitectónica:

```text
C# fuera de Unity = estado, reglas y lógica
Unity = principalmente Vista e interacción
Controladores = puente entre Vista y lógica/aplicación
```

La modalidad vigente del proyecto es **Humano vs Máquina**.

---

## 2. Punto de partida de la Fase 2

La fase comenzó con:

- Fase 0 cerrada;
- Fase 1 cerrada;
- Modelo base implementado y probado;
- `ServicioArchivos` existente;
- modalidad ajustada a Humano vs Máquina;
- arquitectura MVC definida;
- requisito de concurrencia real reservado para una fase posterior;
- necesidad de revisar networking debido al cambio de modalidad.

Durante el trabajo previo a la Vista se consolidó además la separación física:

```text
Unity
  ↓ HTTP/JSON
ImperiosEnGuerra.Api
  ↓
ImperiosEnGuerra.Modelo
  └─ ImperiosEnGuerra.Servicios
```

El Modelo, Servicios y pruebas dejaron de depender de estar dentro de `Assets`.

---

## 3. Arquitectura alcanzada

### 3.1 Proyectos C# externos

```text
src/
├── ImperiosEnGuerra.Modelo/
├── ImperiosEnGuerra.Servicios/
└── ImperiosEnGuerra.Api/

tests/
└── ImperiosEnGuerra.Tests/
```

Características:

- `ImperiosEnGuerra.Modelo`: C# sin `UnityEngine`;
- `ImperiosEnGuerra.Servicios`: responsabilidades transversales como archivos;
- `ImperiosEnGuerra.Api`: ASP.NET Core, HTTP/JSON;
- `ImperiosEnGuerra.Tests`: NUnit externo a Unity.

### 3.2 Unity

Unity quedó principalmente encargado de:

- escena;
- sprites;
- cámara;
- GameObjects;
- renderizado del estado recibido;
- configuración visual;
- herramientas de Editor.

La Vista no decide reglas del juego.

---

## 4. Tiny Swords

Se eligió **Tiny Swords (Free Pack)** de Pixel Frog como pack visual principal.

Mapeo de dominio:

```text
Tiny Swords       Imperios en Guerra
Pawn              Aldeano
Warrior           Guerrero
Lancer            Lancero
Archer            Arquero
Monk              Monje

Blue              Jugador Humano
Red               Jugador Máquina

Gold Stones       Oro
Trees             Madera
Sheep             Comida
```

Jerarquía de unidades vigente:

```text
Unidad
├── Aldeano
├── Soldado
│   ├── Guerrero
│   ├── Lancero
│   └── Arquero
└── Monje
```

Se reemplazaron especializaciones históricas que ya no coincidían con los recursos visuales seleccionados.

---

## 5. Instalación reproducible de Tiny Swords

Se creó:

```text
scripts/instalar_tinyswords.ps1
```

Responsabilidades:

- copiar solamente los assets necesarios;
- mantener fuera del repositorio los gráficos originales del pack;
- evitar `.aseprite` innecesarios;
- verificar archivos clave;
- permitir que otro integrante prepare su entorno local.

La política Git mantiene los PNG externos fuera del repositorio mediante `.gitignore`, mientras los `.meta` necesarios sí se versionan.

También existe:

```text
docs/INSTALACION_TINY_SWORDS.md
```

como guía mínima de instalación.

---

## 6. Configuración de sprites

Se creó una herramienta de Editor para configurar los sprites de Tiny Swords.

La importación se preparó con criterios adecuados para pixel art, incluyendo:

- Sprite 2D/UI según corresponda;
- filtro Point;
- sin compresión innecesaria;
- configuración consistente de unidades, recursos, edificios y tileset.

Al cierre del bloque se verificaron correctamente:

- unidades;
- recursos;
- edificios;
- tileset.

---

## 7. Estado visual expuesto por API

La API pasó de exponer solo un estado mínimo a devolver la información necesaria para representar la partida.

El estado visual incluye:

- dimensiones del mapa;
- recursos físicos del mapa;
- jugador humano;
- jugador máquina;
- recursos almacenados por jugador;
- edificios;
- unidades;
- coordenadas.

Ejemplo conceptual:

```json
{
  "estado": "activa",
  "mapa": {
    "ancho": 10,
    "alto": 8,
    "recursos": []
  },
  "jugadorHumano": {
    "nombre": "Humano",
    "tipo": "Humano",
    "recursos": {
      "oro": 0,
      "madera": 0,
      "comida": 0
    },
    "edificios": [],
    "unidades": []
  },
  "jugadorMaquina": {}
}
```

Se mantiene una diferencia importante:

- recursos físicos del mapa = nodos Oro/Madera/Comida;
- recursos del jugador = saldo almacenado.

---

## 8. DTOs y flujo hacia Unity

El flujo de representación final de Fase 2 es:

```text
Modelo
  ↓
API / mapper
  ↓ JSON
EstadoPartidaDto en Unity
  ↓
VistaPartida
  ↓
GameObjects / SpriteRenderer
```

Unity utiliza DTOs serializables compatibles con `JsonUtility`.

La Vista no consulta directamente al Modelo.

---

## 9. VistaPartida

Se implementó `VistaPartida` como responsable de representar el estado recibido.

Genera una jerarquía visual equivalente a:

```text
VistaPartida
└── ContenidoGenerado
    ├── Mapa
    ├── Recursos
    ├── Edificios
    └── Unidades
```

Representa:

- mapa lógico;
- Oro;
- Madera;
- Comida;
- Centro Urbano Humano;
- Centro Urbano Máquina;
- Aldeano;
- Guerrero;
- Lancero;
- Arquero;
- Monje.

No se agregan unidades ficticias al estado real para demostrar la Vista.

---

## 10. Coordenadas y escala visual

La posición lógica se conserva separada de la escala de presentación.

Transformación visual base:

```text
(x, y) lógico
   ↓
(x * espacioCasilla, y * espacioCasilla) Unity
```

Valores visuales finales de cierre:

```text
espacioCasilla   = 2
escalaRecursos   = 0.75
escalaEdificios  = 0.58
escalaUnidades   = 0.65
```

El suelo se escala para mantener continuidad visual sin modificar las coordenadas lógicas.

Sorting utilizado:

```text
suelo       0
recursos   10
edificios  20
unidades   30
```

---

## 11. Cámara

La cámara ortográfica se ajusta automáticamente al mapa renderizado.

El objetivo es permitir visualizar el estado completo sin incorporar reglas de gameplay a la cámara.

---

## 12. Configurador de la Vista

Se implementó:

```text
Assets/Editor/Fase2VistaConfigurator.cs
```

Menú principal:

```text
Tools
→ Imperios en Guerra
→ Fase 2
→ Configurar vista inicial
```

El configurador es idempotente y se utiliza para:

- localizar/crear `VistaPartida`;
- asignar sprites;
- conectar cámara;
- conectar `ControladorConexionApi`;
- utilizar `http://localhost:5086`;
- guardar la escena.

---

## 13. Herramienta de prueba visual de unidades

Como la partida inicial no contenía unidades, se creó una herramienta exclusivamente de Editor para validar la Vista sin contaminar el Modelo ni la API.

Menú:

```text
Tools
→ Imperios en Guerra
→ Fase 2
→ Probar unidades visuales
```

La herramienta crea un DTO temporal en memoria con:

Humano / Blue:

- Aldeano;
- Guerrero;
- Lancero;
- Arquero;
- Monje.

Máquina / Red:

- Aldeano;
- Guerrero;
- Lancero;
- Arquero;
- Monje.

No modifica la partida real.

---

## 14. Tareas V-00 a V-08

| ID | Tarea | Estado | Resultado |
|---|---|---|---|
| V-00 | Alinear Modelo con Tiny Swords | **TERMINADO** | Jerarquía adaptada a Aldeano, Guerrero, Lancero, Arquero y Monje. |
| V-01 | Preparar Tiny Swords | **TERMINADO** | Instalador, importación y configuración reproducible. |
| V-02 | Exponer estado visual API | **TERMINADO** | API devuelve mapa, jugadores y entidades. |
| V-03 | DTOs / mapper visual | **TERMINADO** | Flujo Partida → API → JSON preparado. |
| V-04 | Mapa visual | **TERMINADO** | Mapa lógico renderizado. |
| V-05 | Recursos visuales | **TERMINADO** | Oro, Madera y Comida representados. |
| V-06 | Edificios visuales | **TERMINADO** | Centros Urbanos Humano/Máquina. |
| V-07 | Unidades visuales | **TERMINADO** | Cinco tipos por ambos bandos validados visualmente. |
| V-08 | Validación y cierre | **TERMINADO** | Pruebas, Unity, Git y PR completados. |

---

## 15. Pruebas y validación

Durante el cierre de Fase 2 se alcanzó:

```text
102/102 pruebas NUnit correctas
```

Además se validó:

- compilación de proyectos .NET;
- compilación de scripts Unity;
- llamadas HTTP manuales;
- mapa en Unity;
- recursos;
- edificios;
- unidades Blue/Red;
- consola Unity sin errores funcionales;
- `git diff --check` limpio antes de integrar.

La advertencia `NU1900` observada durante algunas ejecuciones correspondió a que NuGet no pudo consultar temporalmente su servicio de vulnerabilidades; no representó un fallo de las pruebas del proyecto.

---

## 16. Git y trazabilidad

Bloques principales integrados:

- PR #20 — preparación de Tiny Swords;
- PR #21 — estado visual por API;
- PR #22 — vista inicial de partida en Unity.

La Fase 2 quedó integrada en `develop` antes de comenzar la interacción de Fase 3.

---

## 17. Qué NO quedó implementado en Fase 2

Fase 2 no implementa todavía:

- selección interactiva;
- HUD funcional de gameplay;
- movimiento;
- recolección;
- construcción;
- entrenamiento real;
- ataque;
- IDs estables de entidades;
- IA de la Máquina;
- concurrencia aplicada al gameplay;
- sincronización de datos compartidos del gameplay;
- networking actualizado;
- daño, vida, destrucción y victoria;
- animaciones de gameplay obligatorias.

Estas responsabilidades pertenecen a fases posteriores.

---

## 18. Decisiones que continúan vigentes

- MVC real.
- Modelo y reglas fuera de Unity.
- Unity principalmente como Vista e interacción.
- No concentrar lógica importante en `MonoBehaviour`.
- API como puente entre Unity y el núcleo C# actual.
- No inventar requisitos de gameplay.
- Concurrencia real mediante C# cuando corresponda.
- Workers no modifican directamente `UnityEngine`.
- Centralizar `System.IO`.
- Trabajar mediante ramas, pruebas, commits pequeños y PR hacia `develop`.
- Revisar networking antes de implementarlo debido al cambio Humano vs Máquina.

---

## 19. Punto exacto de continuación al cerrar Fase 2

La siguiente etapa es:

> **FASE 3 — CONTROLADORES, INTERACCIÓN Y ACCIONES**

Objetivo:

> convertir la representación visual en una interfaz interactiva donde Unity capture la intención del usuario y el C# fuera de Unity conserve las reglas y modificaciones reales del estado.

Secuencia prevista:

```text
Selección
  ↓
HUD / acciones
  ↓
Identidad estable
  ↓
Movimiento
  ↓
Recolección
  ↓
Construcción / entrenamiento
  ↓
Ataque
  ↓
Cierre Fase 3
```

---

## 20. Estado final resumido

```text
FASE 0 — TERMINADO
FASE 1 — TERMINADO
FASE 1.5 — TERMINADO
FASE 2 — TERMINADO
FASE 3 — SIGUIENTE

Modalidad:
HUMANO VS MÁQUINA

Arquitectura:
Unity → API → Modelo / Servicios

Vista inicial:
VALIDADA

Pruebas al cierre de Fase 2:
102/102

Siguiente objetivo:
INTERACCIÓN Y GAMEPLAY BASE
```

---

## Fuentes de referencia del estado

Este documento consolida el código y trabajo integrado en `develop`, los PR #20, #21 y #22, las pruebas ejecutadas y las decisiones acumuladas desde las Fases 0 y 1.

Cuando cambie un requisito oficial del profesor, este documento debe preservarse como registro histórico del cierre de Fase 2 y los documentos de fases posteriores deben reflejar el cambio vigente.