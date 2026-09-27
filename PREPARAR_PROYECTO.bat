@echo off
setlocal
cd /d "%~dp0"

echo.
echo ============================================
echo  Preparar Imperios en Guerra
echo ============================================
echo.

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\preparar-proyecto.ps1"

if errorlevel 1 (
    echo.
    echo La preparacion no pudo completarse.
    echo Revise el mensaje anterior.
    pause
    exit /b 1
)

echo.
echo Preparacion completada.
pause
endlocal
