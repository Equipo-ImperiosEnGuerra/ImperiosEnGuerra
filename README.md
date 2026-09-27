# Imperios en Guerra

Proyecto académico de Programación Orientada a Objetos desarrollado en C# y Unity.

**Imperios en Guerra** es un videojuego de estrategia en tiempo real (RTS) inspirado en Age of Empires. La modalidad final del proyecto es **1 jugador Humano vs 3 facciones controladas por Máquina**: Morada, Verde y Amarilla.

> **Alcance actualizado por el docente:** la guía original contemplaba dos jugadores en dos instancias y comunicación obligatoria entre ambas. Posteriormente ese alcance dejó de ser obligatorio para la entrega: la modalidad evaluada pasó a Humano vs Máquina y el networking quedó como componente opcional. El proyecto conserva la implementación de WebSocket + JSON como extensión técnica, pero no depende de un segundo cliente Unity para cumplir el alcance final.

## Tecnologías

- C#
- Unity
- Git y GitHub
- REST
- WebSockets
- JSON
- System.IO
- concurrencia con `Task`, ThreadPool, `CancellationToken` y primitivas thread-safe

## Versión de Unity

**Versión requerida de Unity:** `6000.6.0f1`.

## Inicio rápido

La forma recomendada depende de si se usa un **paquete de Release** ya construido o una copia del **código fuente**.

### Opción A — Ejecutar un paquete de Release

Los paquetes publicados en GitHub ya incluyen el build de escritorio correspondiente, por lo que **no requieren Unity ni Tiny Swords para jugar**.

Requisito común:

- **.NET 10 SDK** instalado y disponible en `PATH`, porque el launcher inicia la API local con `dotnet run`.

#### Windows x64

1. descargue y extraiga `ImperiosEnGuerra-v1.0.0-Entrega-Windows.zip`;
2. abra una terminal en la carpeta extraída;
3. ejecute:

```bat
scripts\ejecutar-juego.bat
```

El launcher:

```text
inicia la API local
        ↓
espera a que responda
        ↓
abre Builds/Windows/ImperiosEnGuerra.exe
        ↓
al cerrar el juego, detiene la API
```

#### Linux x64

1. descargue y extraiga `ImperiosEnGuerra-v1.0.0-Entrega-Linux.zip`;
2. abra una terminal en la carpeta extraída;
3. dé permiso de ejecución al launcher si hace falta:

```bash
chmod +x scripts/ejecutar-juego.sh
```

4. ejecute:

```bash
./scripts/ejecutar-juego.sh
```

El launcher inicia la API local, ejecuta `Builds/Linux/ImperiosEnGuerra.x86_64` y detiene la API al salir.

> Si Linux informa que el ejecutable no tiene permiso, puede ejecutar también:
>
> ```bash
> chmod +x Builds/Linux/ImperiosEnGuerra.x86_64
> ```

### Opción B — Preparar el proyecto desde el código fuente

En una copia recién descargada o clonada, **no abra Unity antes de instalar Tiny Swords**, porque los PNG externos deben quedar asociados a los `.meta` versionados por el proyecto.

#### Windows — código fuente

1. descargue y extraiga **Tiny Swords (Free Pack)** fuera del repositorio;
2. desde la raíz ejecute:

```bat
PREPARAR_PROYECTO.bat
```

3. el asistente comprueba **.NET 10 SDK**, muestra la versión de Unity requerida, protege los `.meta` versionados y solicita la carpeta local de Tiny Swords si hace falta;
4. abra el proyecto con **Unity 6000.6.0f1**;
5. espere a que Unity termine de importar y compilar;
6. genere:

```text
Imperios en Guerra > Build > Build Windows x64
```

7. ejecute:

```bat
scripts\ejecutar-juego.bat
```

#### Linux — código fuente

1. instale **.NET 10 SDK**, **PowerShell (`pwsh`)** y **Unity 6000.6.0f1**;
2. descargue y extraiga **Tiny Swords (Free Pack)** fuera del repositorio;
3. desde la raíz, antes de abrir Unity, instale los gráficos con:

```bash
pwsh ./scripts/instalar_tinyswords.ps1 -Origen "/ruta/a/Tiny Swords (Free Pack)"
```

4. restaure las dependencias de la API:

```bash
dotnet restore src/ImperiosEnGuerra.Api/ImperiosEnGuerra.Api.csproj
```

5. abra el proyecto con **Unity 6000.6.0f1** y espere a que finalice la importación;
6. genere:

```text
Imperios en Guerra > Build > Build Linux x64
```

7. dé permiso de ejecución al launcher:

```bash
chmod +x scripts/ejecutar-juego.sh
```

8. ejecute:

```bash
./scripts/ejecutar-juego.sh
```

### Rutas de build esperadas

```text
Windows:
Builds/Windows/ImperiosEnGuerra.exe

Linux:
Builds/Linux/ImperiosEnGuerra.x86_64
```

`Builds/` no se versiona en Git. Por eso una descarga del **código fuente** necesita generar el ejecutable una vez; los paquetes publicados en **Releases** ya incluyen el build correspondiente.

## Tiny Swords

Los gráficos de **Tiny Swords (Free Pack)** no se redistribuyen dentro del repositorio.

La instalación principal recomendada en una copia nueva se realiza **antes de abrir Unity** mediante `PREPARAR_PROYECTO.bat`. Así los PNG se copian antes de la primera importación y se conservan los GUID de los `.meta` incluidos en Git.

El bootstrap de Unity se mantiene como mecanismo de respaldo/reparación: si detecta assets faltantes durante el trabajo normal, también puede solicitar la carpeta local y reconfigurarlos.

Como alternativa se conserva:

```bat
powershell -ExecutionPolicy Bypass -File scripts\instalar_tinyswords.ps1 -Origen "RUTA_AL_PAQUETE"
```

Guía completa: `docs/INSTALACION_TINY_SWORDS.md`.

## Arquitectura

El proyecto utiliza **Modelo - Vista - Controlador (MVC)**:

- **Modelo:** clases C# POCO con estado, reglas y lógica del juego.
- **Vista:** Unity para escena, sprites, HUD, selección y representación visual.
- **Controlador:** comunica Vista/API con el Modelo y coordina las acciones.

La lógica importante no se concentra en `MonoBehaviour`.

## Concurrencia

Las operaciones largas se ejecutan fuera del Main Thread de Unity:

- movimiento progresivo;
- recolección orgánica;
- construcción progresiva;
- entrenamiento con cola;
- decisiones de la Máquina;
- escucha y envío de networking opcional;
- ataque concurrente.

Se utilizan `Task.Run`, ThreadPool, `CancellationToken`, `lock`, `ConcurrentDictionary`, `ConcurrentQueue`, `Interlocked` y `SemaphoreSlim` según la responsabilidad.

Los workers modifican el **Modelo C#**, nunca directamente `UnityEngine`. Unity consume estados/resultados desde su Main Thread.

Los tiempos del prototipo se mantienen configurables y el combate usa intervalos propios por tipo de unidad.

## Funcionalidades implementadas

El proyecto incluye:

- mapa lógico y recursos físicos de oro, madera y comida;
- Centro Urbano inicial y Aldeanos;
- movimiento progresivo con pathfinding;
- recolección: caminar → extraer → cargar → volver al Centro Urbano → depositar;
- construcción progresiva con reserva de costo/casilla, cancelación y reembolso;
- entrenamiento con costo, cola, progreso y spawn seguro;
- acciones concurrentes de varias unidades;
- una orden activa por unidad;
- combate con vida, daño, alcance e intervalo configurables;
- aproximación automática antes de atacar cuando el objetivo está fuera de alcance;
- ataque contra unidades y edificios enemigos;
- destrucción lógica y liberación de casillas;
- feedback visual de vida crítica;
- condición de victoria/derrota;
- costos económicos del prototipo centralizados en `ConfiguracionEconomia`.

La guía del proyecto no fija valores numéricos de vida, daño, armadura ni alcance por tipo de unidad. Por esta razón, el prototipo utiliza un **balance propio**, centralizado en la configuración del juego.

## Balance de combate del prototipo

| Entidad | Vida | Daño ofensivo | Alcance de ataque | Intervalo de ataque |
|---|---:|---:|---:|---:|
| Aldeano | 60 | 0 | 0 | — |
| Guerrero | 120 | 30 | 1 | 4 s |
| Lancero | 100 | 25 | 1 | 3.5 s |
| Arquero | 80 | 20 | 3 | 3 s |
| Monje | 70 | 0 | 0 | — |
| Centro Urbano | 300 | 0 | 0 | — |

El **Monje no es una unidad ofensiva**. Su función es de apoyo: cura **15 puntos**, a **alcance 2**, con un intervalo de **5 s** entre curaciones.

El combate usa distancia de cuadrícula de 8 vecinos para el alcance, por lo que una diagonal inmediata cuenta como una casilla. El pathfinding continúa usando su movimiento ortogonal original.

## Combate y victoria

Los edificios tienen identidad `Guid` estable y pueden ser objetivo de ataque igual que las unidades.

Flujo base:

```text
unidad militar
→ seleccionar unidad o edificio enemigo
→ calcular si está en alcance
→ si hace falta, aproximarse automáticamente a una casilla válida
→ worker de ataque
→ aplicar daño
→ si Vida > 0, la entidad continúa viva
→ si Vida = 0, destruir y liberar la casilla
→ evaluar victoria
```

La Máquina mantiene un único frente militar activo a la vez para evitar ofensivas simultáneas demasiado rápidas, mientras el resto de la economía continúa siendo concurrente.

La regla terminal fue definida por el equipo como **AND**:

```text
sin Centros Urbanos
Y
sin unidades militares
= derrota
```

Perder únicamente el último Centro Urbano o únicamente la última unidad militar no termina la partida mientras todavía exista el otro conjunto.

Cuando la partida finaliza:

- se registra ganador y motivo en el Modelo;
- se rechazan nuevas órdenes;
- se cancelan workers concurrentes pendientes;
- se detiene el ciclo de IA;
- se genera `resultado_final.txt`;
- la API expone el estado final;
- Unity bloquea selección/clics sobre el mundo;
- aparece una pantalla completa de VICTORIA o DERROTA con botón SALIR.

Las entidades con **25% de vida o menos** se tiñen de rojo como feedback visual.

## Jugador Máquina

La Máquina utiliza las mismas reglas y servicios que el Humano.

Su ciclo de decisiones puede:

1. asignar Aldeanos a recursos;
2. entrenar Aldeanos para crecimiento;
3. construir un segundo Centro Urbano cuando dispone de recursos;
4. reunir recursos para una composición militar objetivo;
5. entrenar fuerza militar diversificada;
6. priorizar objetivos militares humanos;
7. aproximarse según el alcance de la unidad;
8. atacar;
9. asaltar el Centro Urbano cuando ya no hay objetivos militares y dispone de fuerza suficiente.

Las tres facciones usan una doctrina inicial distinta para evitar ejércitos idénticos:

- **Morada:** prioriza Arqueros y luego completa la composición.
- **Verde:** prioriza Lanceros y luego completa la composición.
- **Amarilla:** prioriza Guerreros y luego completa la composición.
- Cuando la economía está desarrollada, las facciones incorporan también Monjes como apoyo.

La IA no abandona su plan solo porque otra unidad sea más barata: puede ordenar a sus Aldeanos recolectar los recursos que necesita para la tropa objetivo. La Máquina no persigue Aldeanos como objetivo militar prioritario y mantiene un solo frente de combate activo.

## Economía visible y spawn

La escena de juego distribuye recursos dejando corredores y casillas de interacción. La configuración utilizada es:

- 8 nodos de Oro;
- 8 nodos de Madera;
- 10 nodos de Comida;
- 26 recursos físicos en total.

El selector de entrenamiento muestra costos compactos:

- `O` = Oro;
- `M` = Madera;
- `C` = Comida.

El spawn de unidades entrenadas evita bordes cuando existe una alternativa interior y prioriza casillas con más salidas libres. Las marcas temporales de ocupación del spawn se liberan al abandonar la casilla para no dejar obstáculos fantasma.

## Comunicación en red

El proyecto conserva una implementación de comunicación mediante **WebSocket + JSON** como componente técnico adicional, junto con la API REST utilizada por Unity. Debido al alcance final Humano vs 3 Máquinas, la ejecución del juego no depende de un segundo cliente Unity.

Endpoint WebSocket:

```text
ws://localhost:5086/ws/partida
```

Estado de red:

```text
GET /api/red/estado
```

Tipos de mensajes soportados por el protocolo:

- `MOVER`
- `RECOLECTAR`
- `CONSTRUIR`
- `ENTRENAR`
- `ATACAR`
- `CURAR`

El listener WebSocket es asíncrono, mantiene clientes en una colección concurrente, usa `SemaphoreSlim` para serializar envíos por conexión, maneja cierres/errores y despacha las acciones hacia `ServicioAccionesConcurrentes`.

## Archivos obligatorios

La responsabilidad de `System.IO` está centralizada en `ServicioArchivos`.

Se generan:

- `configuracion.txt`;
- `log_partida.txt`;
- `resultado_final.txt`.

`resultado_final.txt` incluye la regla de victoria, ganador, perdedores, motivo y una instantánea lógica del estado final: dimensiones del mapa, recursos físicos restantes, saldos, edificios, unidades y obras en curso.

## Ejecutar la API

Desde la raíz del repositorio:

```bash
dotnet run --project src/ImperiosEnGuerra.Api/ImperiosEnGuerra.Api.csproj
```

## Pruebas

Suite .NET:

```bash
dotnet test tests/ImperiosEnGuerra.Tests/ImperiosEnGuerra.Tests.csproj
```

La suite se utiliza como validación principal del Modelo, servicios, concurrencia, networking opcional, ataques, IA y condición de victoria. La última ejecución validada terminó con **390 pruebas correctas, 0 con errores y 0 omitidas**. Los paquetes standalone de **Windows x64 y Linux x64** fueron generados y validados. En ambas plataformas el launcher inicia la API local, abre el juego y detiene la API al cerrar.

Las advertencias de acceso denegado a `log_partida.txt` que aparecen en una prueba son intencionales: esa prueba verifica el manejo controlado de errores de IO.

## Entregables

- Código fuente del proyecto.
- `configuracion.txt`.
- `log_partida.txt`.
- `resultado_final.txt`.
- Informe técnico sobre MVC, concurrencia y comunicación en red.
- Diagrama de clases UML.
- Diagrama de flujo.
- Pruebas de escritorio.

## Documentación

Documentación formal asociada a los entregables:

- `docs/INFORME_TECNICO.md`
- `docs/DIAGRAMA_DE_CLASES.md`
- `docs/DIAGRAMA_DE_FLUJO.md`
- `docs/PRUEBAS_DE_ESCRITORIO.md`

Documentación operativa para preparar los recursos gráficos:

- `docs/INSTALACION_TINY_SWORDS.md`
