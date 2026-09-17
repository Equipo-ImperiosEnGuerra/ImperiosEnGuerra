# ESTADO ACTUAL DEL PROYECTO — IMPERIOS EN GUERRA

## Estado rápido

```text
Fase 0       TERMINADO
Fase 1       TERMINADO
Fase 1.5     TERMINADO
Fase 2       TERMINADO
Fase 3       EN DESARROLLO

Fase 3 C-00/C-07   TERMINADOS
Fase 3 C-08        SIGUIENTE
```

## Punto exacto de continuación

Continuar con:

> **Issue #25 — Fase 3 C-08: agregar identidad estable a las unidades**

Objetivo inmediato:

```text
Unidad.Id
  ↓
API
  ↓
DTO Unity
  ↓
VistaPartida
  ↓
EntidadSeleccionableVista
  ↓
ControladorSeleccion
```

Después:

```text
#26 Movimiento
#27 Recolección base
#28 Construcción + entrenamiento
#29 Ataque + refresco + errores
#30 Cierre Fase 3
#31 Concurrencia y sincronización
#32 Máquina + revisión de networking
#33 Combate + victoria + archivos
#34 Integración final + documentación + pulido
```

## Arquitectura vigente

```text
Unity = entrada + Vista
Controladores Unity = adaptadores delgados
API = puente HTTP/JSON
Modelo C# = reglas y estado
Servicios = responsabilidades transversales
```

Regla:

> Unity captura la intención; C# fuera de Unity valida y modifica el estado.

## Pruebas

Último bloque integrado de Fase 3:

```text
111/111 pruebas NUnit correctas
```

## Documentos de contexto

- `FASE_0_ANALISIS_Y_DISENO_INICIAL.md`
- `FASE_1_AVANCES_Y_ESTADO_CERRADA.md`
- `FASE_2_AVANCES_Y_ESTADO_CERRADA.md`
- `FASE_3_AVANCES_Y_ESTADO_ACTUAL.md`

## Regla para retomar

Cuando se retome el proyecto:

1. no reiniciar fases cerradas;
2. sincronizar `develop`;
3. revisar el Issue pendiente más bajo de la cadena vigente;
4. aplicar requisito → análisis → diseño → implementación → prueba → corrección → documentación → commit;
5. no adelantar concurrencia, IA o combate antes de cerrar sus dependencias;
6. mantener estados `TERMINADO / EN DESARROLLO / PENDIENTE / ERROR`.