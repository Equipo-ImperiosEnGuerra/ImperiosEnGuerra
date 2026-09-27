# Instalación de Tiny Swords

Imperios en Guerra utiliza **Tiny Swords (Free Pack)** de Pixel Frog.

Los sprites no están incluidos en el repositorio, por lo que cada integrante debe disponer de su propia copia del paquete.

## 1. Descargar Tiny Swords

Descargar **Tiny Swords (Free Pack)** desde:

https://pixelfrog-assets.itch.io/tiny-swords

No utilizar una versión distinta del paquete.

## 2. Extraer el ZIP

Extraerlo fuera del repositorio.

Ejemplo:

```text
C:\Proyectos\AssetsExternos\TinySwords\
└── Tiny Swords (Free Pack)\
```

No es necesario copiar manualmente las carpetas a `Assets/`.

## 3. Instalación automática desde Unity — RECOMENDADA

Abrir el proyecto `ImperiosEnGuerra` en Unity.

Si faltan los gráficos locales, el proyecto lo detecta automáticamente y muestra un diálogo:

```text
Imperios en Guerra — Tiny Swords

Faltan los gráficos locales de Tiny Swords.
[Seleccionar carpeta] [Ahora no]
```

Seleccionar:

- directamente la carpeta `Tiny Swords (Free Pack)`; o
- la carpeta padre que contiene `Tiny Swords (Free Pack)`.

El bootstrap del Editor:

1. valida que existan `Buildings`, `Terrain` y `Units`;
2. copia únicamente los gráficos necesarios;
3. refresca `AssetDatabase`;
4. configura unidades, recursos y edificios;
5. configura el tileset y el sub-sprite de suelo;
6. guarda y refresca los assets;
7. confirma que Tiny Swords quedó listo.

No agrega los PNG al repositorio: siguen excluidos mediante `.gitignore`.

## 4. Instalación por terminal — alternativa

El instalador original se mantiene como respaldo.

Desde la raíz del proyecto:

```bat
powershell -ExecutionPolicy Bypass -File scripts\instalar_tinyswords.ps1 -Origen "C:\Proyectos\AssetsExternos\TinySwords"
```

El parámetro `-Origen` puede apuntar a:

- la carpeta que contiene `Tiny Swords (Free Pack)`; o
- directamente a `Tiny Swords (Free Pack)`.

## 5. Verificación

Después de instalar:

1. esperar a que Unity termine de importar;
2. revisar la Console;
3. comprobar que no existan errores;
4. abrir `Assets/Scenes/SampleScene.unity`;
5. ejecutar Play.

La configuración manual desde menús antiguos del Editor ya no es necesaria.

## 6. Funcionamiento técnico

La instalación automática utiliza:

```text
Assets/Editor/TinySwordsBootstrap.cs
```

Al abrir Unity:

```text
¿Están los assets requeridos?
        │
        ├── Sí → no hace nada
        │
        └── No
             ↓
      solicita carpeta
             ↓
      valida paquete
             ↓
      copia gráficos
             ↓
      AssetDatabase.Refresh()
             ↓
      TinySwordsSpriteConfigurator
             ↓
      TinySwordsImporter
             ↓
      proyecto listo
```

En modo batch la ventana automática se desactiva para no bloquear builds o pruebas automatizadas.

## Resumen

```text
Descargar Tiny Swords
        ↓
Extraer ZIP
        ↓
Abrir Unity
        ↓
Seleccionar carpeta cuando se solicite
        ↓
Instalación y configuración automáticas
        ↓
Ejecutar el proyecto
```
