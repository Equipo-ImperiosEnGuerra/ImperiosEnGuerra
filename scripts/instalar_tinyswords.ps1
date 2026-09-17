param(
    [Parameter(Mandatory = $true)]
    [string]$Origen
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "============================================"
Write-Host " Instalador de Tiny Swords - Imperios en Guerra"
Write-Host "============================================"
Write-Host ""

# ---------------------------------------------------------
# Resolver carpeta del paquete
# ---------------------------------------------------------

$origenResuelto = (Resolve-Path $Origen).Path

if (Test-Path (Join-Path $origenResuelto "Tiny Swords (Free Pack)")) {
    $pack = Join-Path $origenResuelto "Tiny Swords (Free Pack)"
}
elseif (
    (Test-Path (Join-Path $origenResuelto "Buildings")) -and
    (Test-Path (Join-Path $origenResuelto "Terrain")) -and
    (Test-Path (Join-Path $origenResuelto "Units"))
) {
    $pack = $origenResuelto
}
else {
    Write-Error @"
No se encontro una instalacion valida de Tiny Swords.

La ruta debe ser:

1. La carpeta que contiene:
   Tiny Swords (Free Pack)

o directamente:

2. La carpeta:
   Tiny Swords (Free Pack)
"@
    exit 1
}

# ---------------------------------------------------------
# Detectar raiz del repositorio
# ---------------------------------------------------------

$repo = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path

$destino = Join-Path $repo "Assets\Art\TinySwords"

New-Item `
    -ItemType Directory `
    -Force `
    -Path $destino |
    Out-Null

Write-Host "Paquete encontrado:"
Write-Host "  $pack"
Write-Host ""

Write-Host "Destino:"
Write-Host "  $destino"
Write-Host ""

# ---------------------------------------------------------
# Carpetas necesarias para Imperios en Guerra
# ---------------------------------------------------------

$carpetas = @(
    @{
        Origen  = "Buildings\Blue Buildings"
        Destino = "Buildings\Blue Buildings"
    },
    @{
        Origen  = "Buildings\Red Buildings"
        Destino = "Buildings\Red Buildings"
    },
    @{
        Origen  = "Terrain\Tileset"
        Destino = "Terrain\Tileset"
    },
    @{
        Origen  = "Terrain\Resources\Gold\Gold Stones"
        Destino = "Terrain\Resources\Gold\Gold Stones"
    },
    @{
        Origen  = "Terrain\Resources\Meat\Sheep"
        Destino = "Terrain\Resources\Meat\Sheep"
    },
    @{
        Origen  = "Terrain\Resources\Wood\Trees"
        Destino = "Terrain\Resources\Wood\Trees"
    },
    @{
        Origen  = "Units\Blue Units"
        Destino = "Units\Blue Units"
    },
    @{
        Origen  = "Units\Red Units"
        Destino = "Units\Red Units"
    }
)

# ---------------------------------------------------------
# Copiar carpetas necesarias
# ---------------------------------------------------------

foreach ($carpeta in $carpetas) {

    $rutaOrigen = Join-Path $pack $carpeta.Origen
    $rutaDestino = Join-Path $destino $carpeta.Destino

    if (-not (Test-Path $rutaOrigen)) {
        Write-Error "No se encontro la carpeta requerida: $rutaOrigen"
        exit 1
    }

    Write-Host "Copiando:"
    Write-Host "  $($carpeta.Origen)"

    New-Item `
        -ItemType Directory `
        -Force `
        -Path $rutaDestino |
        Out-Null

    $archivosGraficos = Get-ChildItem `
        -Path $rutaOrigen `
        -Recurse `
        -File |
        Where-Object {
            $_.Extension.ToLowerInvariant() -in @(".png", ".jpg", ".jpeg")
        }

    foreach ($archivoGrafico in $archivosGraficos) {
        $rutaRelativa = $archivoGrafico.FullName.Substring($rutaOrigen.Length).TrimStart([char[]]"\/")

        $destinoArchivo = Join-Path $rutaDestino $rutaRelativa
        $destinoCarpeta = Split-Path $destinoArchivo -Parent

        New-Item `
            -ItemType Directory `
            -Force `
            -Path $destinoCarpeta |
            Out-Null

        Copy-Item `
            -LiteralPath $archivoGrafico.FullName `
            -Destination $destinoArchivo `
            -Force
    }
}

# ---------------------------------------------------------
# Eliminar archivos .aseprite
# ---------------------------------------------------------

Write-Host ""
Write-Host "Eliminando archivos .aseprite no necesarios..."

$archivosAseprite = Get-ChildItem `
    -Path $destino `
    -Recurse `
    -Filter "*.aseprite" `
    -File `
    -ErrorAction SilentlyContinue

$cantidadAseprite = @($archivosAseprite).Count

foreach ($archivo in $archivosAseprite) {
    Remove-Item `
        -Path $archivo.FullName `
        -Force
}

Write-Host "Archivos .aseprite eliminados:"
Write-Host "  $cantidadAseprite"

# ---------------------------------------------------------
# Verificar archivos principales
# ---------------------------------------------------------

Write-Host ""
Write-Host "Verificando instalacion..."

$archivosRequeridos = @(
    "Buildings\Blue Buildings\Castle.png",
    "Buildings\Red Buildings\Castle.png",
    "Terrain\Tileset\Tilemap_color1.png",
    "Terrain\Resources\Gold\Gold Stones\Gold Stone 1.png",
    "Terrain\Resources\Meat\Sheep\Sheep_Idle.png",
    "Terrain\Resources\Wood\Trees\Tree1.png",
    "Units\Blue Units\Warrior\Warrior_Idle.png",
    "Units\Blue Units\Lancer\Lancer_Idle.png",
    "Units\Blue Units\Archer\Archer_Idle.png",
    "Units\Blue Units\Monk\Idle.png",
    "Units\Blue Units\Pawn\Pawn_Idle.png",
    "Units\Red Units\Warrior\Warrior_Idle.png",
    "Units\Red Units\Lancer\Lancer_Idle.png",
    "Units\Red Units\Archer\Archer_Idle.png",
    "Units\Red Units\Monk\Idle.png",
    "Units\Red Units\Pawn\Pawn_Idle.png"
)

$archivosFaltantes = @()

foreach ($archivoRelativo in $archivosRequeridos) {

    $rutaArchivo = Join-Path $destino $archivoRelativo

    if (-not (Test-Path $rutaArchivo)) {
        $archivosFaltantes += $archivoRelativo
    }
}

if ($archivosFaltantes.Count -gt 0) {

    Write-Host ""
    Write-Host "ERROR: faltan archivos requeridos:"
    Write-Host ""

    foreach ($archivoFaltante in $archivosFaltantes) {
        Write-Host "  - $archivoFaltante"
    }

    Write-Host ""
    Write-Error "La instalacion de Tiny Swords esta incompleta."
    exit 1
}

Write-Host "Verificacion completada correctamente."

# ---------------------------------------------------------
# Resultado final
# ---------------------------------------------------------

Write-Host ""
Write-Host "============================================"
Write-Host " Tiny Swords instalado correctamente."
Write-Host "============================================"
Write-Host ""
Write-Host "Ahora abra el proyecto en Unity."
Write-Host ""
Write-Host "Los archivos graficos originales estan"
Write-Host "excluidos del repositorio mediante .gitignore."
Write-Host ""
Write-Host "El proyecto utiliza los PNG exportados."
Write-Host "Los archivos fuente .aseprite no se conservan"
Write-Host "dentro de Assets."
Write-Host ""