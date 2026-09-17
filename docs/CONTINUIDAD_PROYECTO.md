# CONTINUIDAD DEL PROYECTO

Este archivo funciona como índice rápido para integrantes que retomen el proyecto.

## Documentos principales

1. `FASE_0_ANALISIS_Y_DISENO_INICIAL.md`
2. `FASE_1_AVANCES_Y_ESTADO_CERRADA.md`
3. `FASE_2_AVANCES_Y_ESTADO_CERRADA.md`
4. `FASE_3_AVANCES_Y_ESTADO_ACTUAL.md`
5. `ESTADO_ACTUAL_PROYECTO.md`

## Regla de continuidad

No reiniciar fases cerradas.

El punto actual de continuación es:

> Issue #25 — C-08 Identidad estable de unidades.

Después continuar en orden con los Issues #26 a #34 respetando sus dependencias.

## Arquitectura

```text
Unity -> API -> Modelo / Servicios
```

- Unity: entrada y Vista.
- Controladores Unity: adaptadores delgados.
- API: comunicación HTTP/JSON.
- Modelo: reglas y estado.
- Servicios: responsabilidades transversales.

## Metodología

```text
requisito
→ análisis
→ diseño
→ implementación
→ prueba
→ corrección
→ documentación
→ commit
```

Mantener estados:

- TERMINADO
- EN DESARROLLO
- PENDIENTE
- ERROR
