$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$apiProject = Join-Path $root "src/ImperiosEnGuerra.Api/ImperiosEnGuerra.Api.csproj"
$gamePath = Join-Path $root "Builds/Windows/ImperiosEnGuerra.exe"
$logsDir = Join-Path $root "Logs"
$apiLog = Join-Path $logsDir "api-runtime.log"

if (-not (Test-Path $apiProject)) {
    throw "No se encontró la API en: $apiProject"
}

if (-not (Test-Path $gamePath)) {
    throw "No existe el build de Windows. Genéralo desde Unity: Imperios en Guerra > Build > Build Windows x64"
}

New-Item -ItemType Directory -Force -Path $logsDir | Out-Null

$api = $null
$game = $null

try {
    Write-Host "Iniciando API..."
    $api = Start-Process -FilePath "dotnet" -ArgumentList @("run", "--project", $apiProject) -WorkingDirectory $root -RedirectStandardOutput $apiLog -RedirectStandardError $apiLog -PassThru

    Start-Sleep -Seconds 3

    if ($api.HasExited) {
        throw "La API terminó antes de iniciar el juego. Revisa $apiLog"
    }

    Write-Host "Iniciando Imperios en Guerra..."
    $game = Start-Process -FilePath $gamePath -WorkingDirectory (Split-Path -Parent $gamePath) -PassThru

    $game.WaitForExit()
}
finally {
    if ($api -ne $null -and -not $api.HasExited) {
        Write-Host "Cerrando API..."
        Stop-Process -Id $api.Id -Force -ErrorAction SilentlyContinue
    }
}
