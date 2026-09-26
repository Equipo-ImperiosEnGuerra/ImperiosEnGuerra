$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$apiProject = Join-Path $root "src/ImperiosEnGuerra.Api/ImperiosEnGuerra.Api.csproj"
$gamePath = Join-Path $root "Builds/Windows/ImperiosEnGuerra.exe"
$logsDir = Join-Path $root "Logs"
$apiOutLog = Join-Path $logsDir "api-runtime.out.log"
$apiErrLog = Join-Path $logsDir "api-runtime.err.log"
$apiHealthUrl = "http://localhost:5086/api/red/estado"

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
    $argumentosApi = "run --project `"$apiProject`""
    $api = Start-Process -FilePath "dotnet" -ArgumentList $argumentosApi -WorkingDirectory $root -RedirectStandardOutput $apiOutLog -RedirectStandardError $apiErrLog -PassThru

    $apiLista = $false

    for ($intento = 0; $intento -lt 30; $intento++) {
        if ($api.HasExited) {
            throw "La API terminó antes de iniciar el juego. Revisa $apiErrLog"
        }

        try {
            Invoke-WebRequest -UseBasicParsing -Uri $apiHealthUrl -TimeoutSec 1 | Out-Null
            $apiLista = $true
            break
        }
        catch {
            Start-Sleep -Milliseconds 500
        }
    }

    if (-not $apiLista) {
        throw "La API no respondió en $apiHealthUrl. Revisa $apiOutLog y $apiErrLog"
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
