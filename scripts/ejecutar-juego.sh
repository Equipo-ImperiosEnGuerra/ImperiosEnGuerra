#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
API_PROJECT="$ROOT/src/ImperiosEnGuerra.Api/ImperiosEnGuerra.Api.csproj"
GAME="$ROOT/Builds/Linux/ImperiosEnGuerra.x86_64"
LOG_DIR="$ROOT/Logs"
API_LOG="$LOG_DIR/api-runtime.log"

if [[ ! -f "$API_PROJECT" ]]; then
  echo "No se encontró la API en: $API_PROJECT" >&2
  exit 1
fi

if [[ ! -f "$GAME" ]]; then
  echo "No existe el build de Linux." >&2
  echo "Genéralo desde Unity: Imperios en Guerra > Build > Build Linux x64" >&2
  exit 1
fi

mkdir -p "$LOG_DIR"

cleanup() {
  if [[ -n "${API_PID:-}" ]] && kill -0 "$API_PID" 2>/dev/null; then
    kill "$API_PID" 2>/dev/null || true
    wait "$API_PID" 2>/dev/null || true
  fi
}
trap cleanup EXIT INT TERM

echo "Iniciando API..."
(
  cd "$ROOT"
  dotnet run --project "$API_PROJECT"
) >"$API_LOG" 2>&1 &
API_PID=$!

sleep 3

if ! kill -0 "$API_PID" 2>/dev/null; then
  echo "La API terminó antes de iniciar el juego. Revisa $API_LOG" >&2
  exit 1
fi

echo "Iniciando Imperios en Guerra..."
chmod +x "$GAME" 2>/dev/null || true
"$GAME"
