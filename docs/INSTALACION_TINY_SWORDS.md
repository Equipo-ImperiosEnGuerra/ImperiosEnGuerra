# Instalación de Tiny Swords

Imperios en Guerra utiliza **Tiny Swords (Free Pack)** de Pixel Frog.

Los sprites no están incluidos en el repositorio, por lo que deben descargarse antes de abrir el proyecto en Unity.

## 1. Descargar Tiny Swords

Descargar **Tiny Swords (Free Pack)** desde:

https://pixelfrog-assets.itch.io/tiny-swords

No utilizar la versión antigua del paquete.

## 2. Extraer el ZIP

Extraer el paquete fuera del repositorio.

Ejemplo:

```text
C:\Proyectos\AssetsExternos\TinySwords\
└── Tiny Swords (Free Pack)\
```

No copiar manualmente las carpetas dentro de Unity.

## 3. Ejecutar el instalador

Abrir CMD o PowerShell en la raíz del proyecto:

```text
C:\Proyectos\ImperiosEnGuerra>
```

Ejecutar:

```bat
powershell -ExecutionPolicy Bypass -File scripts\instalar_tinyswords.ps1 -Origen "C:\Proyectos\AssetsExternos\TinySwords"
```

Si Tiny Swords fue extraído en otra ubicación, cambiar únicamente la ruta indicada en `-Origen`.

El script copiará automáticamente los archivos necesarios al proyecto.

## 4. Verificar

Si todo salió correctamente aparecerá:

```text
============================================
 Tiny Swords instalado correctamente.
============================================

Ahora abra el proyecto en Unity.
```

## 5. Abrir Unity

1. Abrir Unity Hub.
2. Abrir el proyecto `ImperiosEnGuerra`.
3. Esperar a que Unity termine de importar y compilar.
4. Revisar la Console.
5. Verificar que no existan errores.

No es necesario configurar manualmente los sprites. El proyecto incluye los archivos `.meta` y herramientas necesarias para conservar su configuración.

## Resumen

```text
Descargar Tiny Swords
        ↓
Extraer ZIP
        ↓
Ejecutar instalar_tinyswords.ps1
        ↓
Abrir Unity
        ↓
Esperar importación
        ↓
Ejecutar el proyecto
```
