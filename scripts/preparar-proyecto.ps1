$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$instaladorTinySwords = Join-Path $PSScriptRoot "instalar_tinyswords.ps1"
$apiProject = Join-Path $root "src\ImperiosEnGuerra.Api\ImperiosEnGuerra.Api.csproj"
$projectVersion = Join-Path $root "ProjectSettings\ProjectVersion.txt"
$tinyDestino = Join-Path $root "Assets\Art\TinySwords"

$archivosTinySwords = @(
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

function Write-Step([string]$mensaje) {
    Write-Host ""
    Write-Host "==> $mensaje"
}

function Test-TinySwordsCompleto {
    foreach ($archivo in $archivosTinySwords) {
        if (-not (Test-Path (Join-Path $tinyDestino $archivo))) {
            return $false
        }
    }

    return $true
}

function Assert-MetasVersionados {
    $faltantes = @()

    foreach ($archivo in $archivosTinySwords) {
        $meta = Join-Path $tinyDestino ($archivo + ".meta")

        if (-not (Test-Path $meta)) {
            $faltantes += $meta
        }
    }

    if ($faltantes.Count -gt 0) {
        throw @"
Faltan archivos .meta versionados de Tiny Swords.

Para proteger los GUID de Unity, use una copia limpia/actualizada del repositorio
antes de instalar los PNG.

Primer archivo faltante:
$($faltantes[0])
"@
    }
}

function Restore-TinySwordsMetasFromGit {
    $git = Get-Command git -ErrorAction SilentlyContinue

    if ($null -eq $git) {
        Write-Host "Git no esta disponible; se conservaran los .meta presentes en la copia descargada."
        return
    }

    $gitDir = Join-Path $root ".git"

    if (-not (Test-Path $gitDir)) {
        Write-Host "La copia no es un clone de Git; se conservaran los .meta incluidos en la descarga."
        return
    }

    Write-Host "Restaurando .meta versionados de Tiny Swords para conservar GUID..."

    & git -C $root restore -- "Assets/Art/TinySwords" 2>$null

    if ($LASTEXITCODE -ne 0) {
        throw "Git no pudo restaurar los .meta de Tiny Swords."
    }
}

function Select-TinySwordsFolder {
    try {
        Add-Type -AssemblyName System.Windows.Forms

        $dialogo = New-Object System.Windows.Forms.FolderBrowserDialog
        $dialogo.Description =
            "Seleccione Tiny Swords (Free Pack), o la carpeta que lo contiene."
        $dialogo.ShowNewFolderButton = $false

        $resultado = $dialogo.ShowDialog()

        if ($resultado -eq [System.Windows.Forms.DialogResult]::OK) {
            return $dialogo.SelectedPath
        }

        return $null
    }
    catch {
        Write-Warning "No fue posible abrir el selector grafico de carpetas."
        $ruta = Read-Host "Pegue la ruta de Tiny Swords (Free Pack)"

        if ([string]::IsNullOrWhiteSpace($ruta)) {
            return $null
        }

        return $ruta.Trim('"')
    }
}

function Assert-Dotnet10 {
    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue

    if ($null -eq $dotnet) {
        throw @"
No se encontro 'dotnet' en PATH.

Instale .NET 10 SDK y vuelva a ejecutar PREPARAR_PROYECTO.bat.
"@
    }

    $sdks = @(& dotnet --list-sdks)

    $tieneNet10 =
        $sdks |
        Where-Object { $_ -match '^10\.' } |
        Select-Object -First 1

    if ($null -eq $tieneNet10) {
        throw @"
No se encontro .NET 10 SDK.

SDK detectados:
$($sdks -join [Environment]::NewLine)

Instale .NET 10 SDK y vuelva a ejecutar PREPARAR_PROYECTO.bat.
"@
    }

    Write-Host "OK: .NET 10 SDK detectado."
}

function Show-UnityVersion {
    if (-not (Test-Path $projectVersion)) {
        Write-Warning "No se encontro ProjectSettings\ProjectVersion.txt."
        return
    }

    $linea =
        Get-Content $projectVersion |
        Where-Object { $_ -like "m_EditorVersion:*" } |
        Select-Object -First 1

    if ($null -ne $linea) {
        $version = ($linea -split ":", 2)[1].Trim()
        Write-Host "Version Unity requerida por el proyecto: $version"
    }
}

Write-Host ""
Write-Host "============================================"
Write-Host " Preparacion inicial - Imperios en Guerra"
Write-Host "============================================"
Write-Host ""
Write-Host "Este asistente debe ejecutarse antes del primer arranque de Unity"
Write-Host "en una copia nueva del repositorio."
Write-Host ""

Write-Step "1/4 - Verificando .NET"
Assert-Dotnet10

Write-Step "2/4 - Verificando version de Unity"
Show-UnityVersion

Write-Step "3/4 - Preparando Tiny Swords"

if (Test-TinySwordsCompleto) {
    Write-Host "OK: Tiny Swords ya esta instalado."
}
else {
    if (-not (Test-Path $instaladorTinySwords)) {
        throw "No se encontro el instalador: $instaladorTinySwords"
    }

    Restore-TinySwordsMetasFromGit
    Assert-MetasVersionados

    Write-Host ""
    Write-Host "Faltan los graficos locales de Tiny Swords."
    Write-Host "Por licencia no se incluyen en el repositorio."
    Write-Host ""

    $origen = Select-TinySwordsFolder

    if ([string]::IsNullOrWhiteSpace($origen)) {
        throw "No se selecciono una carpeta de Tiny Swords."
    }

    & $instaladorTinySwords -Origen $origen

    if ($LASTEXITCODE -ne 0) {
        throw "El instalador de Tiny Swords termino con error."
    }

    if (-not (Test-TinySwordsCompleto)) {
        throw "Tiny Swords no quedo completo despues de la instalacion."
    }

    Assert-MetasVersionados

    Write-Host "OK: Tiny Swords instalado antes de abrir Unity."
}

Write-Step "4/4 - Restaurando dependencias .NET"

if (-not (Test-Path $apiProject)) {
    throw "No se encontro el proyecto API: $apiProject"
}

& dotnet restore $apiProject

if ($LASTEXITCODE -ne 0) {
    throw "dotnet restore termino con error."
}

Write-Host ""
Write-Host "============================================"
Write-Host " PROYECTO PREPARADO CORRECTAMENTE"
Write-Host "============================================"
Write-Host ""
Write-Host "Siguiente paso:"
Write-Host ""
Write-Host "1. Abra el proyecto en Unity Hub con la version indicada arriba."
Write-Host "2. Espere a que Unity importe y compile."
Write-Host "3. Genere el build:"
Write-Host "   Imperios en Guerra > Build > Build Windows x64"
Write-Host "4. Ejecute:"
Write-Host "   scripts\ejecutar-juego.bat"
Write-Host ""
