# Build y ejecución de escritorio

## Objetivo

Este documento describe cómo generar y ejecutar una versión de escritorio de **Imperios en Guerra** sin modificar las reglas de gameplay ni la arquitectura estable del proyecto.

El juego mantiene esta separación:

```text
Unity = Vista
API .NET = servicios, concurrencia y acceso al Modelo
Modelo C# = estado y reglas del juego
```

El build de Unity necesita que la API esté ejecutándose en:

```text
http://localhost:5086
```

---

## 1. Requisitos

- Unity **6000.6.0f1**.
- .NET SDK compatible con `net10.0`.
- Módulo de build de Unity correspondiente a la plataforma de destino.
- Repositorio actualizado.

La escena incluida en el build es la que esté habilitada en:

```text
File > Build Profiles
```

Actualmente el proyecto tiene habilitada:

```text
Assets/Scenes/SampleScene.unity
```

---

## 2. Generar build desde Unity

Abrir el proyecto en Unity y esperar a que termine de compilar.

En el menú superior aparecerá:

```text
Imperios en Guerra
└── Build
    ├── Build plataforma actual
    ├── Build Windows x64
    └── Build Linux x64
```

### Windows

Seleccionar:

```text
Imperios en Guerra > Build > Build Windows x64
```

Salida:

```text
Builds/Windows/ImperiosEnGuerra.exe
```

### Linux

Seleccionar:

```text
Imperios en Guerra > Build > Build Linux x64
```

Salida:

```text
Builds/Linux/ImperiosEnGuerra.x86_64
```

La carpeta `Builds/` está ignorada por Git porque contiene artefactos generados localmente.

Si Unity no tiene instalado el módulo de la plataforma solicitada, el build fallará y debe instalarse ese módulo desde Unity Hub.

---

## 3. Ejecutar en Windows con un solo acceso

Después de generar el build de Windows:

```bat
scripts\ejecutar-juego.bat
```

El lanzador:

1. comprueba que exista el build;
2. inicia la API con `dotnet run`;
3. espera hasta que `http://localhost:5086/api/red/estado` responda;
4. inicia `ImperiosEnGuerra.exe`;
5. cuando se cierra el juego, detiene la API iniciada por el script.

Los logs de arranque de la API quedan en:

```text
Logs/api-runtime.out.log
Logs/api-runtime.err.log
```

---

## 4. Ejecutar en Linux

Después de generar el build de Linux:

```bash
bash scripts/ejecutar-juego.sh
```

El lanzador inicia la API, abre el build y detiene la API al cerrar el juego.

El log se guarda en:

```text
Logs/api-runtime.log
```

---

## 5. Ejecución manual

La API también puede iniciarse manualmente desde la raíz:

```bash
dotnet run --project src/ImperiosEnGuerra.Api/ImperiosEnGuerra.Api.csproj
```

Después se puede abrir el ejecutable generado desde `Builds/`.

---

## 6. Smoke test de entrega

Después de generar el build, verificar como mínimo:

```text
1. Abre el menú inicial.
2. Abre y cierra las instrucciones.
3. Inicia una partida.
4. Selecciona un Aldeano.
5. Mueve y recolecta un recurso.
6. Entrena una unidad.
7. Comprueba que las tres IAs mantengan economía y actividad militar.
8. Ejecuta al menos un combate.
9. Verifica que no existan errores bloqueantes.
10. Cierra el juego y confirma que el proceso de API iniciado por el lanzador termina.
```

Para una validación de estabilidad final se recomienda mantener una partida activa durante varios minutos con economía, entrenamiento y combate concurrentes.

---

## 7. Alcance

La herramienta de build y los scripts de lanzamiento son infraestructura de entrega.

No modifican:

- Modelo;
- reglas de victoria;
- balance;
- IA;
- concurrencia de gameplay;
- networking;
- sincronización;
- Vista de la partida.

Por tanto, la versión jugable sigue utilizando exactamente la misma lógica validada antes de generar el build.
