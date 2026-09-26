@echo off
setlocal
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0ejecutar-juego.ps1"
if errorlevel 1 (
    echo.
    echo No fue posible iniciar Imperios en Guerra.
    pause
)
endlocal
