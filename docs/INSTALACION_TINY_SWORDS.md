# Instalación de Tiny Swords

Imperios en Guerra utiliza **Tiny Swords (Free Pack)** de Pixel Frog.

Los sprites no se incluyen en el repositorio. Los archivos `.meta` sí se versionan para conservar los GUID y referencias de Unity.

## 1. Regla para una copia nueva

En una PC nueva o después de descargar/clonar el repositorio:

> **Ejecute `PREPARAR_PROYECTO.bat` antes de abrir Unity por primera vez.**

Esto permite copiar los PNG antes de que Unity procese sus `.meta` y reduce el riesgo de regenerar GUID.

## 2. Descargar y extraer Tiny Swords

Descargue **Tiny Swords (Free Pack)** y extráigalo fuera del repositorio.

Ejemplo:

```text
C:\Proyectos\AssetsExternos\TinySwords\
└── Tiny Swords (Free Pack)\
```

La carpeta válida puede ser:

- directamente `Tiny Swords (Free Pack)`; o
- la carpeta padre que la contiene.

## 3. Preparación recomendada en Windows

Desde el Explorador o desde CMD, ejecute en la raíz del repositorio:

```bat
PREPARAR_PROYECTO.bat
```

El asistente realiza:

```text
comprobar .NET 10 SDK
        ↓
mostrar versión Unity requerida
        ↓
¿Tiny Swords ya está completo?
   ├── Sí → continuar
   └── No
        ↓
restaurar .meta versionados si existe Git
        ↓
solicitar carpeta Tiny Swords
        ↓
copiar solo gráficos necesarios
        ↓
verificar archivos
        ↓
dotnet restore de la API
        ↓
proyecto preparado
```

La instalación de Tiny Swords reutiliza:

```text
scripts/instalar_tinyswords.ps1
```

Los PNG continúan excluidos mediante `.gitignore`.

## 4. Después de preparar

Abra el proyecto con:

```text
Unity 6000.6.0f1
```

Espere a que termine la importación.

Las herramientas internas del Editor configuran automáticamente sprites y tileset cuando corresponde.

Para generar el juego:

```text
Imperios en Guerra > Build > Build Windows x64
```

Después:

```bat
scripts\ejecutar-juego.bat
```

## 5. Bootstrap de Unity como respaldo

El proyecto conserva:

```text
Assets/Editor/TinySwordsBootstrap.cs
```

Si durante el desarrollo detecta que falta algún asset, puede solicitar nuevamente la carpeta del paquete y reparar la instalación.

Este mecanismo es un **respaldo**. Para una clonación limpia se recomienda el preparador externo porque actúa antes del primer arranque de Unity.

## 6. Instalación manual por terminal

Si no se desea usar el preparador general:

```bat
powershell -ExecutionPolicy Bypass -File scripts\instalar_tinyswords.ps1 -Origen "C:\Proyectos\AssetsExternos\TinySwords"
```

Debe ejecutarse antes de abrir Unity en una copia nueva.

## 7. Verificación

Antes de abrir Unity, puede comprobar por ejemplo:

```bat
dir "Assets\Art\TinySwords\Buildings\Blue Buildings\Castle*"
```

Debe existir el par:

```text
Castle.png
Castle.png.meta
```

La misma idea aplica a unidades, recursos y tileset.

## 8. Resumen

```text
Descargar/clonar repositorio
        ↓
Descargar + extraer Tiny Swords
        ↓
PREPARAR_PROYECTO.bat
        ↓
Abrir Unity 6000.6.0f1
        ↓
Build Windows x64
        ↓
scripts\ejecutar-juego.bat
```
