# Validación final de entrega — Imperios en Guerra

Este documento define la última validación manual que debe realizarse sobre la versión estable de `main` antes de cerrar el PR documental y congelar la entrega.

## 1. Preparación

Desde la raíz del proyecto:

```bat
git switch main
git pull origin main
git status
```

Resultado esperado:

```text
On branch main
Your branch is up to date with 'origin/main'.
nothing to commit, working tree clean
```

Ejecutar nuevamente la suite si hubo cualquier cambio de código posterior:

```bat
dotnet test tests\ImperiosEnGuerra.Tests\ImperiosEnGuerra.Tests.csproj
```

Referencia de la última ejecución validada:

```text
390 correctas
0 errores
0 omitidas
```

## 2. Iniciar la API

```bat
dotnet run --project src\ImperiosEnGuerra.Api\ImperiosEnGuerra.Api.csproj
```

La API debe quedar disponible en:

```text
http://localhost:5086
```

No cerrar esta consola mientras se valida desde Unity.

## 3. Smoke test desde Unity

Abrir el proyecto con Unity **6000.6.0f1**.

Abrir:

```text
Assets/Scenes/SampleScene.unity
```

Pulsar **Play** y comprobar, en orden:

### VF-01 — Menú inicial

- el menú aparece correctamente;
- botones visibles;
- no hay errores bloqueantes en Console.

### VF-02 — Instrucciones

- abrir instrucciones;
- comprobar que el texto sea legible;
- cerrar y volver al menú.

### VF-03 — Inicio de partida

- iniciar partida;
- aparece el Humano;
- aparecen Morada, Verde y Amarilla;
- aparecen Centros Urbanos;
- aparecen recursos físicos;
- HUD muestra Oro, Madera y Comida.

### VF-04 — Aldeano humano inactivo

Dejar un Aldeano humano sin orden.

Resultado esperado:

- permanece quieto;
- después del tiempo configurado el HUD puede avisar que existe un Aldeano quieto/disponible.

### VF-05 — Aldeano IA inactivo

Observar una IA con Aldeano libre.

Resultado esperado:

- puede hacer un paseo corto;
- movimiento de una sola casilla ortogonal;
- no crea una segunda recolección;
- el paseo no reemplaza una acción económica real.

### VF-06 — Movimiento humano

Ordenar a una unidad moverse varias casillas.

Resultado esperado:

- avanza progresivamente;
- no se teletransporta;
- no entra en bucles;
- la interfaz continúa respondiendo.

### VF-07 — Recolección

Ordenar a un Aldeano recolectar.

Resultado esperado:

- se aproxima al recurso;
- el recurso muestra feedback visual;
- extrae;
- vuelve al Centro Urbano;
- deposita;
- el HUD aumenta el saldo.

### VF-08 — Construcción

Construir un Centro Urbano cuando existan recursos suficientes.

Resultado esperado:

- aparece la obra;
- progresa;
- termina en edificio;
- no aparece duplicado;
- no deja casillas fantasma.

### VF-09 — Entrenamiento

Entrenar al menos:

- un Aldeano;
- una unidad militar.

Resultado esperado:

- se descuenta el costo;
- existe progreso/espera;
- la unidad aparece en una casilla válida;
- no se superpone con otra entidad.

### VF-10 — Comportamiento IA

Observar durante varios ciclos.

Resultado esperado:

- cada IA mantiene economía;
- no todas entrenan únicamente Guerreros;
- Morada favorece Arqueros;
- Verde favorece Lanceros;
- Amarilla favorece Guerreros;
- puede aparecer Monje cuando la economía lo permite;
- se mantiene máximo un frente ofensivo normal por facción.

### VF-11 — Defensa reactiva

Atacar una unidad militar IA que no sea la unidad del frente ofensivo normal.

Resultado esperado:

- la unidad exacta atacada puede responder;
- unidades cercanas no se suman automáticamente;
- no se generan múltiples frentes defensivos extra de la misma facción.

### VF-12 — Combate y vida crítica

Atacar hasta dejar una entidad en 25% de vida o menos.

Resultado esperado:

- la vida disminuye;
- la entidad se tiñe de rojo;
- el bando no cambia;
- al llegar a 0 la entidad desaparece lógicamente y visualmente.

### VF-13 — Pausa

Abrir el menú de pausa.

Resultado esperado:

- interacción del mundo bloqueada;
- IA/reacciones/regeneración detenidas;
- workers en curso no avanzan mientras la pausa temporal está activa;
- al reanudar, la partida continúa.

### VF-14 — Eliminación parcial de IA

Eliminar completamente una IA.

Resultado esperado:

- solo se considera eliminada si queda sin Centros Urbanos **y** sin unidades militares;
- el Humano puede recibir el Centro Urbano de conquista según la lógica actual;
- las otras IAs siguen activas;
- no aparece victoria prematura.

### VF-15 — Victoria final

Eliminar las tres IAs.

Resultado esperado:

- partida finalizada;
- interacción bloqueada;
- pantalla **VICTORIA**;
- botones de menú/salir;
- no se ejecutan nuevas órdenes.

### VF-16 — Derrota — VALIDADO

En otra partida de prueba, permitir que el Humano quede sin Centros Urbanos y sin unidades militares.

Resultado esperado:

- pantalla **DERROTA**;
- interacción bloqueada;
- no se ejecutan nuevas órdenes.

**Evidencia obtenida:** se validó visualmente la pantalla de DERROTA en una partida real, con el motivo coherente con la regla AND y botones VOLVER AL MENÚ / SALIR visibles.

## 4. Verificar archivos generados

Los archivos se generan en:

```text
src/ImperiosEnGuerra.Api/DatosPartida/
```

Comprobar:

### configuracion.txt — VALIDADO

Validado manualmente sobre una partida real:

- mapa 15x15;
- 1 jugador Humano y 3 IAs;
- 1 Centro Urbano inicial por facción;
- 2 Aldeanos iniciales por facción;
- saldos iniciales;
- 8 nodos de Oro;
- 8 nodos de Madera;
- 10 nodos de Comida;
- 26 recursos físicos compartidos.

### log_partida.txt — FUNCIONALMENTE VALIDADO

Se revisó una ejecución real y el archivo registra correctamente:

- establecimiento de partidas;
- entrenamiento;
- construcción;
- ataques exitosos y rechazados;
- curación exitosa y rechazada;
- regeneración de recursos;
- conquista de facciones;
- finalización y ganador.

El archivo revisado es acumulativo y contiene sesiones históricas de versiones anteriores. Por ello, para la evidencia final de entrega debe generarse un log limpio ejecutando una partida nueva sobre la versión final de `main`.

La lectura con PowerShell usando `-Encoding UTF8` confirmó que el archivo contiene correctamente caracteres como `daño`, `curación`, `está` y `quedó`. La salida corrupta observada con `type` en CMD era únicamente un problema de página de códigos de la consola, no del archivo.

### resultado_final.txt — VALIDADO

Validado manualmente sobre una derrota real.

Se comprobó:

- `Estado=Finalizada`;
- `ReglaVictoria=AND`;
- ganador `CPU Morada`;
- perdedor Humano;
- motivo de derrota;
- sección `[MAPA_FINAL]`;
- dimensiones 15x15;
- recursos físicos restantes con cantidad;
- sección final del Humano;
- secciones finales de las 3 IAs;
- saldos finales;
- edificios sobrevivientes;
- unidades sobrevivientes y vida;
- obras en curso.

La derrota observada es coherente con la regla AND: el Humano conserva Aldeanos, pero queda sin Centros Urbanos y sin unidades militares.

Conservar una copia de los tres archivos como evidencia de entrega.

## 5. Validar build standalone — BUILD WINDOWS VALIDADO

### Windows

Desde Unity:

```text
Imperios en Guerra
→ Build
→ Build Windows x64
```

Debe crearse:

```text
Builds/Windows/ImperiosEnGuerra.exe
```

Después cerrar la API manual que estuviera ejecutándose y probar:

```bat
scripts\ejecutar-juego.bat
```

Validar:

1. el launcher inicia la API;
2. espera disponibilidad;
3. inicia el juego;
4. el juego puede iniciar una partida;
5. al cerrar el juego, la API iniciada por el launcher termina.

**Evidencia obtenida:** el build Windows fue generado y ejecutado correctamente mediante el launcher; abrió el juego y permitió iniciar/interactuar con una partida. También se corrigió y verificó visualmente el HUD standalone. Queda pendiente confirmar explícitamente el cierre automático del proceso de API al salir del juego.

## 6. Evidencias recomendadas

Guardar capturas de:

1. menú inicial;
2. partida con las 3 IAs;
3. recolección;
4. construcción;
5. entrenamiento;
6. combate;
7. vida crítica;
8. pausa;
9. victoria;
10. contenido de `resultado_final.txt`.

## 7. Criterio de cierre

La entrega queda lista para congelarse cuando:

```text
[ ] Suite .NET final sin errores
[x] Unity/build sin errores bloqueantes durante la validación realizada
[ ] Smoke test completo
[x] configuracion.txt verificado
[ ] log_partida.txt verificado con evidencia limpia final
[x] resultado_final.txt verificado
[x] Build standalone generado
[~] Launcher probado: inicio OK; falta confirmar cierre automático de API
[ ] Evidencias guardadas
[ ] Documentación consistente con main
[ ] PR documental listo para merge
```
