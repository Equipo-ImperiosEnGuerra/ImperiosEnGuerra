using System;
using System.Collections.Generic;
using System.Threading;
using ImperiosEnGuerra.Modelo.Unidades;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Combate;
using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Servicios.Concurrencia;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Movimiento;
using ImperiosEnGuerra.Modelo.Recoleccion;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Core;

namespace ImperiosEnGuerra.Api.Servicios;

public sealed class ServicioAccionesConcurrentes
{
    private readonly EstadoPartidaService estadoPartida;
    private readonly GestorProcesosConcurrentes gestorProcesos;
    private readonly ServicioOrdenesUnidad servicioOrdenes;

    private readonly TimeSpan retardoMovimiento;
    private readonly TimeSpan retardoRecoleccion;
    private readonly TimeSpan retardoConstruccion;
    private readonly TimeSpan retardoEntrenamiento;
    private readonly TimeSpan retardoAtaque;
    private readonly ConfiguracionRecoleccion configuracionRecoleccion;
    private readonly ConfiguracionEntrenamiento configuracionEntrenamiento;
    private bool usarIntervaloCombateConfigurado;


    // ============================================================
    // CONSTRUCTOR USADO POR LA API MEDIANTE INYECCIÓN DE DEPENDENCIAS
    // ============================================================
    //
    // Mantiene los tiempos de demostración actuales del proyecto:
    //
    // Movimiento:      1 segundo
    // Recolección:     1 segundo
    // Construcción:    7 segundos
    // Entrenamiento:   5 segundos
    // Ataque:          4 segundos
    //
    public ServicioAccionesConcurrentes(
        EstadoPartidaService estadoPartida,
        GestorProcesosConcurrentes gestorProcesos,
        ServicioOrdenesUnidad servicioOrdenes)
        : this(
            estadoPartida,
            gestorProcesos,
            servicioOrdenes,
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(7),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(4))
    {
        usarIntervaloCombateConfigurado = true;
    }


    // ============================================================
    // CONSTRUCTOR UTILIZADO PRINCIPALMENTE POR LAS PRUEBAS
    // ============================================================
    //
    // Permite usar, por ejemplo:
    //
    // TimeSpan.Zero
    // TimeSpan.FromMilliseconds(5)
    // TimeSpan.FromMilliseconds(100)
    // TimeSpan.FromSeconds(10)
    //
    // Así las pruebas no tienen que esperar los tiempos reales
    // del prototipo.
    //
    public ServicioAccionesConcurrentes(
        EstadoPartidaService estadoPartida,
        GestorProcesosConcurrentes gestorProcesos,
        TimeSpan retardoDemostracion)
        : this(
            estadoPartida,
            gestorProcesos,
            new ServicioOrdenesUnidad(),
            retardoDemostracion,
            retardoDemostracion,
            retardoDemostracion,
            retardoDemostracion,
            retardoDemostracion)
    {
    }


    // ============================================================
    // CONSTRUCTOR COMPLETO
    // ============================================================
    //
    // Centraliza la configuración y validación de dependencias
    // y retardos.
    //
    public ServicioAccionesConcurrentes(
        EstadoPartidaService estadoPartida,
        GestorProcesosConcurrentes gestorProcesos,
        ServicioOrdenesUnidad servicioOrdenes,
        TimeSpan retardoMovimiento,
        TimeSpan retardoRecoleccion,
        TimeSpan retardoConstruccion,
        TimeSpan retardoEntrenamiento,
        TimeSpan retardoAtaque)
    {
        this.estadoPartida =
            estadoPartida
            ?? throw new ArgumentNullException(
                nameof(estadoPartida));

        this.gestorProcesos =
            gestorProcesos
            ?? throw new ArgumentNullException(
                nameof(gestorProcesos));

        this.servicioOrdenes =
            servicioOrdenes
            ?? throw new ArgumentNullException(
                nameof(servicioOrdenes));

        ValidarRetardo(
            retardoMovimiento,
            nameof(retardoMovimiento));

        ValidarRetardo(
            retardoRecoleccion,
            nameof(retardoRecoleccion));

        ValidarRetardo(
            retardoConstruccion,
            nameof(retardoConstruccion));

        ValidarRetardo(
            retardoEntrenamiento,
            nameof(retardoEntrenamiento));

        ValidarRetardo(
            retardoAtaque,
            nameof(retardoAtaque));

        this.retardoMovimiento = retardoMovimiento;
        this.retardoRecoleccion = retardoRecoleccion;
        this.retardoConstruccion = retardoConstruccion;
        this.retardoEntrenamiento = retardoEntrenamiento;
        this.retardoAtaque = retardoAtaque;
        configuracionRecoleccion =
            new ConfiguracionRecoleccion();

        configuracionEntrenamiento =
            new ConfiguracionEntrenamiento();

        this.estadoPartida.PartidaFinalizada +=
            CancelarTodos;
    }


    // ============================================================
    // MOVIMIENTO
    // ============================================================

    public ProcesoConcurrente IniciarMovimiento(
        MoverUnidadRequest? request)
    {
        MoverUnidadRequest? copia =
            Copiar(request);

        return gestorProcesos.Iniciar(
            "MOVER",
            token =>
            {
                if (!Guid.TryParse(
                        copia?.UnidadId,
                        out Guid unidadId))
                {
                    return ResultadoAccion.Fallido(
                        "El ID de la unidad debe tener formato Guid válido.");
                }

                Unidad? unidad =
                    estadoPartida.ObtenerUnidad(
                        unidadId);

                if (unidad == null)
                {
                    return ResultadoAccion.Fallido(
                        "No existe una unidad humana con ese ID.");
                }

                TimeSpan retardoPaso =
                    CalcularRetardoPasoMovimiento(
                        retardoMovimiento,
                        unidad.VelocidadMovimiento);

                ResultadoPlanMovimiento plan =
                    estadoPartida.PrepararMovimientoProgresivo(
                        copia);

                if (!plan.Exito)
                {
                    token.ThrowIfCancellationRequested();

                    return ResultadoAccion.Fallido(
                        plan.Mensaje);
                }

                token.ThrowIfCancellationRequested();

                if (plan.Pasos.Count == 0)
                {
                    return ResultadoAccion.Exitoso(
                        "Movimiento realizado.");
                }

                bool ordenIniciada =
                    estadoPartida.IntentarIniciarOrdenUnidad(
                        unidadId,
                        TipoAccionJuego.Mover);

                if (!ordenIniciada)
                {
                    token.ThrowIfCancellationRequested();

                    return ResultadoAccion.Fallido(
                        "La unidad no está disponible.");
                }

                try
                {
                    const int maximoReplanes = 12;
                    int replanteos = 0;

                    var pasos =
                        new Queue<Coordenada>(
                            plan.Pasos);

                    while (pasos.Count > 0)
                    {
                        EsperarAntesDeAplicar(
                            token,
                            retardoPaso);

                        token.ThrowIfCancellationRequested();

                        Coordenada siguiente =
                            pasos.Dequeue();

                        ResultadoAccion resultadoPaso =
                            estadoPartida.AvanzarMovimiento(
                                unidadId,
                                siguiente);

                        if (!resultadoPaso.Exito)
                        {
                            replanteos++;

                            if (replanteos > maximoReplanes)
                            {
                                return ResultadoAccion.Fallido(
                                    "Movimiento detenido para evitar un bucle de colisiones entre unidades. " +
                                    "La unidad queda libre para recibir una nueva orden.");
                            }

                            EsperarCesionPaso(
                                unidadId,
                                token,
                                replanteos);

                            ResultadoPlanMovimiento
                                nuevoPlan =
                                    estadoPartida
                                        .PrepararMovimientoProgresivo(
                                            copia,
                                            true);

                            if (!nuevoPlan.Exito)
                            {
                                return ResultadoAccion.Fallido(
                                    nuevoPlan.Mensaje);
                            }

                            pasos =
                                new Queue<Coordenada>(
                                    nuevoPlan.Pasos);

                            continue;
                        }

                        Console.WriteLine(
                            $"MOVIMIENTO_PASO: {unidadId} -> " +
                            $"({siguiente.X},{siguiente.Y})");
                    }

                    return ResultadoAccion.Exitoso(
                        "Movimiento realizado.");
                }
                finally
                {
                    estadoPartida.CompletarOrdenUnidad(
                        unidadId);
                }
            });
    }

    // ============================================================
    // RECOLECCIÓN
    // ============================================================

    public ProcesoConcurrente IniciarRecoleccion(
        RecolectarRequest? request)
    {
        RecolectarRequest? copia =
            Copiar(request);

        return gestorProcesos.Iniciar(
            "RECOLECTAR",
            token =>
            {
                if (!Guid.TryParse(
                        copia?.AldeanoId,
                        out Guid unidadId))
                {
                    return ResultadoAccion.Fallido(
                        "El ID del Aldeano debe tener formato Guid válido.");
                }

                if (copia?.Objetivo == null)
                {
                    return ResultadoAccion.Fallido(
                        "El objetivo de recolección es obligatorio.");
                }

                Unidad? unidad =
                    estadoPartida.ObtenerUnidad(
                        unidadId);

                if (!(unidad is Aldeano aldeano))
                {
                    return ResultadoAccion.Fallido(
                        "La unidad seleccionada no es un Aldeano.");
                }

                Coordenada objetivo =
                    new Coordenada(
                        copia.Objetivo.X,
                        copia.Objetivo.Y);

                TimeSpan retardoPaso =
                    CalcularRetardoPasoMovimiento(
                        retardoMovimiento,
                        aldeano.VelocidadMovimiento);

                int totalDepositado = 0;
                bool ordenIniciada = false;
                ResultadoAproximacionRecurso planInicial;

                try
                {
                    // Política de cancelación recuperable:
                    // si el Aldeano conserva una carga de una orden anterior,
                    // la deposita antes de intentar una nueva extracción.
                    if (aldeano.CargaActual > 0)
                    {
                        ResultadoAproximacionDeposito cargaPendiente =
                            PrepararAproximacionDepositoConReintentos(
                                unidadId,
                                token);

                        if (!cargaPendiente.Exito)
                        {
                            return ResultadoAccion.Fallido(
                                cargaPendiente.Mensaje);
                        }

                        TipoAccionJuego ordenCarga =
                            cargaPendiente.Pasos.Count > 0
                                ? TipoAccionJuego.Mover
                                : TipoAccionJuego.Recolectar;

                        if (!estadoPartida.IntentarIniciarOrdenUnidad(
                                unidadId,
                                ordenCarga))
                        {
                            return ResultadoAccion.Fallido(
                                "El Aldeano no está disponible.");
                        }

                        ordenIniciada = true;

                        ResultadoAccion regresoPendiente =
                            EjecutarHaciaDepositoConReplan(
                                unidadId,
                                cargaPendiente,
                                retardoPaso,
                                token,
                                out cargaPendiente);

                        if (!regresoPendiente.Exito)
                            return regresoPendiente;

                        if (!estadoPartida.IntentarReemplazarOrdenUnidad(
                                unidadId,
                                TipoAccionJuego.Recolectar))
                        {
                            return ResultadoAccion.Fallido(
                                "No se pudo activar la fase de depósito pendiente.");
                        }

                        ResultadoDepositoRecoleccion depositoPendiente =
                            estadoPartida.DepositarCarga(
                                unidadId,
                                cargaPendiente.CentroUrbano);

                        if (!depositoPendiente.Exito)
                        {
                            return ResultadoAccion.Fallido(
                                depositoPendiente.Mensaje);
                        }

                        totalDepositado +=
                            depositoPendiente.CantidadDepositada;

                        Console.WriteLine(
                            $"RECOLECCION_DEPOSITO_PENDIENTE: {unidadId} " +
                            $"+{depositoPendiente.CantidadDepositada} " +
                            $"{depositoPendiente.TipoRecurso}");

                        if (!estadoPartida.IntentarReemplazarOrdenUnidad(
                                unidadId,
                                TipoAccionJuego.Mover))
                        {
                            return ResultadoAccion.Fallido(
                                "No se pudo iniciar el regreso al recurso.");
                        }

                        planInicial =
                            PrepararAproximacionRecursoConReintentos(
                                copia,
                                token,
                                true);

                        if (!planInicial.Exito)
                        {
                            if (estadoPartida.RecursoExiste(
                                    unidadId,
                                    objetivo) &&
                                !estadoPartida.RecursoDisponible(
                                    unidadId,
                                    objetivo))
                            {
                                return ResultadoAccion.Exitoso(
                                    $"La carga pendiente fue depositada ({totalDepositado}). " +
                                    "El nodo objetivo ya está agotado.");
                            }

                            return ResultadoAccion.Fallido(
                                planInicial.Mensaje);
                        }
                    }
                    else
                    {
                        planInicial =
                            PrepararAproximacionRecursoConReintentos(
                                copia,
                                token);

                        if (!planInicial.Exito)
                        {
                            return ResultadoAccion.Fallido(
                                planInicial.Mensaje);
                        }

                        TipoAccionJuego ordenInicial =
                            planInicial.Pasos.Count > 0
                                ? TipoAccionJuego.Mover
                                : TipoAccionJuego.Recolectar;

                        if (!estadoPartida.IntentarIniciarOrdenUnidad(
                                unidadId,
                                ordenInicial))
                        {
                            return ResultadoAccion.Fallido(
                                "El Aldeano no está disponible.");
                        }

                        ordenIniciada = true;
                    }

                    if (!planInicial.TipoRecurso.HasValue)
                    {
                        return ResultadoAccion.Fallido(
                            "No se pudo determinar el tipo de recurso objetivo.");
                    }

                    ResultadoAccion movimientoInicial =
                        EjecutarHaciaRecursoConReplan(
                            copia,
                            unidadId,
                            planInicial,
                            retardoPaso,
                            token,
                            out planInicial);

                    if (!movimientoInicial.Exito)
                        return movimientoInicial;

                    TipoRecurso tipoObjetivo =
                        planInicial.TipoRecurso.Value;

                    while (true)
                    {
                        token.ThrowIfCancellationRequested();

                        if (!estadoPartida.IntentarReemplazarOrdenUnidad(
                                unidadId,
                                TipoAccionJuego.Recolectar))
                        {
                            return ResultadoAccion.Fallido(
                                "No se pudo activar la fase de recolección.");
                        }

                        int tasa =
                            configuracionRecoleccion.ObtenerTasa(
                                tipoObjetivo);

                        bool recursoAgotado = false;

                        while (aldeano.CapacidadDisponible > 0)
                        {
                            EsperarAntesDeAplicar(
                                token,
                                retardoRecoleccion);

                            ResultadoPasoRecoleccion paso =
                                estadoPartida.RecolectarPaso(
                                    unidadId,
                                    objetivo,
                                    tasa);

                            if (!paso.Exito)
                            {
                                return ResultadoAccion.Fallido(
                                    paso.Mensaje);
                            }

                            Console.WriteLine(
                                $"RECOLECCION_PASO: {unidadId} +{paso.CantidadExtraida} " +
                                $"{paso.TipoRecurso} carga={paso.CargaActual}/{paso.CapacidadCarga}");

                            recursoAgotado =
                                paso.RecursoAgotado;

                            if (paso.CapacidadCompleta ||
                                paso.RecursoAgotado ||
                                paso.CantidadExtraida == 0)
                            {
                                break;
                            }
                        }

                        if (aldeano.CargaActual > 0)
                        {
                            if (!estadoPartida.IntentarReemplazarOrdenUnidad(
                                    unidadId,
                                    TipoAccionJuego.Mover))
                            {
                                return ResultadoAccion.Fallido(
                                    "No se pudo iniciar el regreso al depósito.");
                            }

                            ResultadoAproximacionDeposito depositoPlan =
                                PrepararAproximacionDepositoConReintentos(
                                    unidadId,
                                    token);

                            if (!depositoPlan.Exito)
                            {
                                return ResultadoAccion.Fallido(
                                    depositoPlan.Mensaje);
                            }

                            ResultadoAccion regreso =
                                EjecutarHaciaDepositoConReplan(
                                    unidadId,
                                    depositoPlan,
                                    retardoPaso,
                                    token,
                                    out depositoPlan);

                            if (!regreso.Exito)
                                return regreso;

                            if (!estadoPartida.IntentarReemplazarOrdenUnidad(
                                    unidadId,
                                    TipoAccionJuego.Recolectar))
                            {
                                return ResultadoAccion.Fallido(
                                    "No se pudo activar la fase de depósito.");
                            }

                            ResultadoDepositoRecoleccion deposito =
                                estadoPartida.DepositarCarga(
                                    unidadId,
                                    depositoPlan.CentroUrbano);

                            if (!deposito.Exito)
                            {
                                return ResultadoAccion.Fallido(
                                    deposito.Mensaje);
                            }

                            totalDepositado +=
                                deposito.CantidadDepositada;

                            Console.WriteLine(
                                $"RECOLECCION_DEPOSITO: {unidadId} " +
                                $"+{deposito.CantidadDepositada} {deposito.TipoRecurso}");
                        }

                        if (recursoAgotado ||
                            !estadoPartida.RecursoDisponible(
                                unidadId,
                                objetivo))
                        {
                            return ResultadoAccion.Exitoso(
                                $"Recolección completada. Se depositaron {totalDepositado} " +
                                $"de {tipoObjetivo} y el nodo quedó agotado.");
                        }

                        if (!estadoPartida.IntentarReemplazarOrdenUnidad(
                                unidadId,
                                TipoAccionJuego.Mover))
                        {
                            return ResultadoAccion.Fallido(
                                "No se pudo iniciar el regreso al recurso.");
                        }

                        ResultadoAproximacionRecurso nuevoPlan =
                            PrepararAproximacionRecursoConReintentos(
                                copia,
                                token,
                                true);

                        if (!nuevoPlan.Exito)
                        {
                            if (!estadoPartida.RecursoDisponible(
                                    unidadId,
                                    objetivo))
                            {
                                return ResultadoAccion.Exitoso(
                                    $"Recolección completada. Se depositaron " +
                                    $"{totalDepositado} de {tipoObjetivo}.");
                            }

                            return ResultadoAccion.Fallido(
                                nuevoPlan.Mensaje);
                        }

                        ResultadoAccion regresoRecurso =
                            EjecutarHaciaRecursoConReplan(
                                copia,
                                unidadId,
                                nuevoPlan,
                                retardoPaso,
                                token,
                                out nuevoPlan);

                        if (!regresoRecurso.Exito)
                            return regresoRecurso;
                    }
                }
                finally
                {
                    if (ordenIniciada)
                    {
                        estadoPartida.CompletarOrdenUnidad(
                            unidadId);
                    }
                }
            });
    }

    // ============================================================
    // CONSTRUCCIÓN
    // ============================================================

    public ProcesoConcurrente IniciarConstruccion(
        ConstruirRequest? request)
    {
        ConstruirRequest? copia =
            Copiar(request);

        return gestorProcesos.Iniciar(
            "CONSTRUIR",
            token =>
            {
                CostoRecursos costo = null;
                bool costoReservado = false;
                bool completada = false;
                Guid obraId = Guid.Empty;
                Guid unidadId = Guid.Empty;
                Unidad? unidad = null;

                try
                {
                    if (!Guid.TryParse(
                            copia?.AldeanoId,
                            out unidadId))
                    {
                        return ResultadoAccion.Fallido(
                            "El ID del Aldeano debe tener formato Guid válido.");
                    }

                    ResultadoAccion reserva =
                        estadoPartida.ReservarCostoConstruccion(
                            unidadId,
                            copia?.TipoEdificio,
                            out costo);

                    if (!reserva.Exito)
                        return reserva;

                    costoReservado = true;

                    ResultadoAccion inicioObra =
                        estadoPartida.IniciarObra(
                            copia,
                            out obraId);

                    if (!inicioObra.Exito)
                        return inicioObra;

                    unidad =
                        estadoPartida.ObtenerUnidad(
                            unidadId);

                    if (!(unidad is Aldeano aldeano))
                    {
                        return ResultadoAccion.Fallido(
                            "La unidad seleccionada no es un Aldeano.");
                    }

                    ResultadoAproximacionConstruccion plan =
                        estadoPartida.PrepararAproximacionConstruccion(
                            unidadId,
                            obraId);

                    if (!plan.Exito)
                        return ResultadoAccion.Fallido(plan.Mensaje);

                    TipoAccionJuego ordenInicial =
                        plan.Pasos.Count > 0
                            ? TipoAccionJuego.Mover
                            : TipoAccionJuego.Construir;

                    if (!estadoPartida.IntentarIniciarOrdenUnidad(
                            unidadId,
                            ordenInicial))
                    {
                        return ResultadoAccion.Fallido(
                            "El Aldeano no está disponible.");
                    }

                    TimeSpan retardoPaso =
                        CalcularRetardoPasoMovimiento(
                            retardoMovimiento,
                            aldeano.VelocidadMovimiento);

                    ResultadoAccion movimiento =
                        EjecutarHaciaObraConReplan(
                            unidadId,
                            obraId,
                            plan,
                            retardoPaso,
                            token);

                    if (!movimiento.Exito)
                        return movimiento;

                    if (!estadoPartida.IntentarReemplazarOrdenUnidad(
                            unidadId,
                            TipoAccionJuego.Construir))
                    {
                        return ResultadoAccion.Fallido(
                            "No se pudo iniciar la construcción.");
                    }

                    const int pasosProgreso = 10;
                    TimeSpan retardoProgreso =
                        DividirRetardo(
                            retardoConstruccion,
                            pasosProgreso);

                    for (int i = 0;
                         i < pasosProgreso;
                         i++)
                    {
                        EsperarAntesDeAplicar(
                            token,
                            retardoProgreso);

                        ResultadoProgresoConstruccion progreso =
                            estadoPartida.AvanzarObra(
                                obraId,
                                10);

                        if (!progreso.Exito)
                        {
                            return ResultadoAccion.Fallido(
                                progreso.Mensaje);
                        }

                        Console.WriteLine(
                            $"CONSTRUCCION_PROGRESO: {obraId} {progreso.Progreso}%");

                        if (progreso.Terminada)
                        {
                            completada = true;
                            break;
                        }
                    }

                    if (!completada)
                    {
                        return ResultadoAccion.Fallido(
                            "La construcción no alcanzó el 100%.");
                    }

                    return ResultadoAccion.Exitoso(
                        "Construcción terminada correctamente.");
                }
                finally
                {
                    if (!completada &&
                        obraId != Guid.Empty)
                    {
                        estadoPartida.CancelarObra(
                            obraId);
                    }

                    if (!completada &&
                        costoReservado)
                    {
                        estadoPartida.ReembolsarCosto(
                            unidadId,
                            costo);
                    }

                    if (unidad != null)
                    {
                        estadoPartida.CompletarOrdenUnidad(
                            unidad.Id);
                    }
                }
            });
    }

    // ============================================================
    // ENTRENAMIENTO
    // ============================================================

    public ProcesoConcurrente IniciarEntrenamiento(
        EntrenarRequest? request)
    {
        EntrenarRequest? copia =
            Copiar(request);

        return gestorProcesos.Iniciar(
            "ENTRENAR",
            token =>
            {
                CostoRecursos costo = null;
                bool costoReservado = false;
                bool completado = false;
                Guid entrenamientoId = Guid.Empty;
                Coordenada centroUrbano = null;
                TipoJugador propietarioTipo =
                    TipoJugador.Humano;

                try
                {
                    ResultadoAccion reserva =
                        estadoPartida.ReservarCostoEntrenamiento(
                            copia,
                            out costo,
                            out propietarioTipo);

                    if (!reserva.Exito)
                        return reserva;

                    costoReservado = true;

                    ResultadoAccion encolado =
                        estadoPartida.EncolarEntrenamiento(
                            copia,
                            propietarioTipo,
                            out entrenamientoId,
                            out centroUrbano);

                    if (!encolado.Exito)
                        return encolado;

                    while (!estadoPartida.EsTurnoEntrenamiento(
                        centroUrbano,
                        entrenamientoId,
                        propietarioTipo))
                    {
                        EsperarAntesDeAplicar(
                            token,
                            TimeSpan.FromMilliseconds(10));
                    }

                    if (!configuracionEntrenamiento
                        .IntentarObtenerFactor(
                            copia?.TipoUnidad,
                            out double factor))
                    {
                        return ResultadoAccion.Fallido(
                            "No existe tiempo configurado para la unidad.");
                    }

                    TimeSpan tiempoTotal =
                        MultiplicarRetardo(
                            retardoEntrenamiento,
                            factor);

                    TimeSpan retardoProgreso =
                        DividirRetardo(
                            tiempoTotal,
                            10);

                    for (int i = 0;
                         i < 10;
                         i++)
                    {
                        EsperarAntesDeAplicar(
                            token,
                            retardoProgreso);

                        ResultadoProgresoEntrenamiento progreso =
                            estadoPartida.AvanzarEntrenamiento(
                                centroUrbano,
                                entrenamientoId,
                                10,
                                propietarioTipo);

                        if (!progreso.Exito)
                        {
                            return ResultadoAccion.Fallido(
                                progreso.Mensaje);
                        }

                        Console.WriteLine(
                            $"ENTRENAMIENTO_PROGRESO: {entrenamientoId} " +
                            $"{progreso.Progreso}%");
                    }

                    ResultadoSpawnEntrenamiento spawn =
                        estadoPartida.CompletarEntrenamientoConSpawn(
                            centroUrbano,
                            entrenamientoId,
                            copia?.TipoUnidad ?? string.Empty,
                            propietarioTipo);

                    if (!spawn.Exito)
                    {
                        return ResultadoAccion.Fallido(
                            spawn.Mensaje);
                    }

                    completado = true;

                    return ResultadoAccion.Exitoso(
                        $"{spawn.Mensaje} Spawn ({spawn.Coordenada.X},{spawn.Coordenada.Y}).");
                }
                finally
                {
                    if (!completado &&
                        entrenamientoId != Guid.Empty &&
                        centroUrbano != null)
                    {
                        estadoPartida.CancelarEntrenamientoCola(
                            centroUrbano,
                            entrenamientoId,
                            propietarioTipo);
                    }

                    if (!completado &&
                        costoReservado)
                    {
                        estadoPartida.ReembolsarCosto(
                            propietarioTipo,
                            costo);
                    }
                }
            });
    }

    // ============================================================
    // ATAQUE
    // ============================================================

    public ProcesoConcurrente IniciarAtaque(
        AtacarRequest? request)
    {
        AtacarRequest? copia =
            Copiar(request);

        return gestorProcesos.Iniciar(
            "ATACAR",
            token =>
            {
                if (!Guid.TryParse(
                        copia?.AtacanteId,
                        out Guid unidadId))
                {
                    return ResultadoAccion.Fallido(
                        "El ID del atacante debe tener formato Guid válido.");
                }

                Unidad? unidad =
                    estadoPartida.ObtenerUnidad(
                        unidadId);

                if (unidad == null)
                {
                    return ResultadoAccion.Fallido(
                        "No existe la unidad atacante indicada.");
                }

                ResultadoAproximacionAtaque aproximacion =
                    estadoPartida.PrepararAproximacionAtaque(
                        copia);

                if (!aproximacion.Exito)
                {
                    return ResultadoAccion.Fallido(
                        aproximacion.Mensaje);
                }

                bool ordenIniciada = false;

                try
                {
                    if (aproximacion.Pasos.Count > 0)
                    {
                        if (!estadoPartida.IntentarIniciarOrdenUnidad(
                                unidadId,
                                TipoAccionJuego.Mover))
                        {
                            return ResultadoAccion.Fallido(
                                "La unidad atacante no está disponible.");
                        }

                        ordenIniciada = true;

                        TimeSpan retardoPaso =
                            CalcularRetardoPasoMovimiento(
                                retardoMovimiento,
                                unidad.VelocidadMovimiento);

                        foreach (Coordenada paso in aproximacion.Pasos)
                        {
                            EsperarAntesDeAplicar(
                                token,
                                retardoPaso);

                            token.ThrowIfCancellationRequested();

                            ResultadoAccion movimiento =
                                estadoPartida.AvanzarMovimiento(
                                    unidadId,
                                    paso);

                            if (!movimiento.Exito)
                            {
                                return ResultadoAccion.Fallido(
                                    $"No se pudo aproximar al objetivo: {movimiento.Mensaje}");
                            }
                        }

                        if (!estadoPartida.IntentarReemplazarOrdenUnidad(
                                unidadId,
                                TipoAccionJuego.Atacar))
                        {
                            return ResultadoAccion.Fallido(
                                "No se pudo cambiar de aproximación a ataque.");
                        }
                    }
                    else
                    {
                        if (!estadoPartida.IntentarIniciarOrdenUnidad(
                                unidadId,
                                TipoAccionJuego.Atacar))
                        {
                            return ResultadoAccion.Fallido(
                                "La unidad atacante no está disponible.");
                        }

                        ordenIniciada = true;
                    }

                    TimeSpan esperaAtaque =
                        usarIntervaloCombateConfigurado
                            ? TimeSpan.FromSeconds(
                                estadoPartida.ObtenerIntervaloAtaqueSegundos(
                                    unidadId))
                            : retardoAtaque;

                    EsperarAntesDeAplicar(
                        token,
                        esperaAtaque);

                    return estadoPartida.Atacar(
                        copia);
                }
                finally
                {
                    if (ordenIniciada)
                    {
                        estadoPartida.CompletarOrdenUnidad(
                            unidadId);
                    }
                }
            });
    }


    // ============================================================
    // CANCELACIÓN
    // ============================================================

    public bool Cancelar(
        Guid procesoId)
    {
        return gestorProcesos.Cancelar(
            procesoId);
    }


    public void CancelarTodos()
    {
        gestorProcesos.CancelarTodos();
    }


    // ============================================================
    // RESULTADOS DE LOS PROCESOS
    // ============================================================

    public bool IntentarObtenerResultado(
        Guid procesoId,
        out ResultadoProcesoConcurrente resultado)
    {
        return gestorProcesos.IntentarObtenerResultado(
            procesoId,
            out resultado);
    }


    public bool IntentarObtenerResultado(
        out ResultadoProcesoConcurrente resultado)
    {
        return gestorProcesos.IntentarObtenerResultado(
            out resultado);
    }


    public int ProcesosActivos =>
        gestorProcesos.ProcesosActivos;


    private ResultadoAproximacionRecurso
        PrepararAproximacionRecursoConReintentos(
            RecolectarRequest? request,
            CancellationToken token,
            bool permitirOrdenMovimientoActiva = false)
    {
        const int maximoIntentos = 8;

        ResultadoAproximacionRecurso ultimoResultado =
            ResultadoAproximacionRecurso.Fallido(
                "No se pudo preparar la aproximación al recurso.");

        for (int intento = 1;
             intento <= maximoIntentos;
             intento++)
        {
            token.ThrowIfCancellationRequested();

            ultimoResultado =
                estadoPartida.PrepararAproximacionRecurso(
                    request,
                    permitirOrdenMovimientoActiva);

            if (ultimoResultado.Exito ||
                !ultimoResultado.Reintentable ||
                intento == maximoIntentos)
            {
                return ultimoResultado;
            }

            EsperarAntesDeAplicar(
                token,
                retardoMovimiento);
        }

        return ultimoResultado;
    }

    private ResultadoAproximacionDeposito
        PrepararAproximacionDepositoConReintentos(
            Guid aldeanoId,
            CancellationToken token)
    {
        const int maximoIntentos = 8;

        ResultadoAproximacionDeposito ultimoResultado =
            ResultadoAproximacionDeposito.Fallido(
                "No se pudo preparar la aproximación al depósito.");

        for (int intento = 1;
             intento <= maximoIntentos;
             intento++)
        {
            token.ThrowIfCancellationRequested();

            ultimoResultado =
                estadoPartida.PrepararAproximacionDeposito(
                    aldeanoId,
                    true);

            if (ultimoResultado.Exito ||
                !ultimoResultado.Reintentable ||
                intento == maximoIntentos)
            {
                return ultimoResultado;
            }

            EsperarAntesDeAplicar(
                token,
                retardoMovimiento);
        }

        return ultimoResultado;
    }

    private ResultadoAccion EjecutarHaciaRecursoConReplan(
        RecolectarRequest? request,
        Guid unidadId,
        ResultadoAproximacionRecurso planInicial,
        TimeSpan retardoPaso,
        CancellationToken token,
        out ResultadoAproximacionRecurso planFinal)
    {
        const int maximoReplanes = 12;
        planFinal = planInicial;

        for (int intento = 1;
             intento <= maximoReplanes;
             intento++)
        {
            token.ThrowIfCancellationRequested();

            ResultadoAccion movimiento =
                EjecutarPasosRecoleccion(
                    unidadId,
                    planFinal.Pasos,
                    retardoPaso,
                    token);

            if (movimiento.Exito)
                return movimiento;

            if (intento == maximoReplanes)
                break;

            EsperarCesionPaso(
                unidadId,
                token,
                intento);

            planFinal =
                PrepararAproximacionRecursoConReintentos(
                    request,
                    token,
                    true);

            if (!planFinal.Exito)
            {
                return ResultadoAccion.Fallido(
                    planFinal.Mensaje);
            }

            Console.WriteLine(
                $"RECOLECCION_REPLAN: {unidadId} intento {intento + 1}");
        }

        return ResultadoAccion.Fallido(
            "No se encontró una ruta libre hacia el recurso tras varios cambios del mapa. " +
            "La orden terminó sin bloquear los demás workers.");
    }

    private ResultadoAccion EjecutarHaciaDepositoConReplan(
        Guid unidadId,
        ResultadoAproximacionDeposito planInicial,
        TimeSpan retardoPaso,
        CancellationToken token,
        out ResultadoAproximacionDeposito planFinal)
    {
        const int maximoReplanes = 12;
        planFinal = planInicial;

        for (int intento = 1;
             intento <= maximoReplanes;
             intento++)
        {
            token.ThrowIfCancellationRequested();

            ResultadoAccion movimiento =
                EjecutarPasosRecoleccion(
                    unidadId,
                    planFinal.Pasos,
                    retardoPaso,
                    token);

            if (movimiento.Exito)
                return movimiento;

            if (intento == maximoReplanes)
                break;

            EsperarCesionPaso(
                unidadId,
                token,
                intento);

            planFinal =
                PrepararAproximacionDepositoConReintentos(
                    unidadId,
                    token);

            if (!planFinal.Exito)
            {
                return ResultadoAccion.Fallido(
                    planFinal.Mensaje);
            }

            Console.WriteLine(
                $"DEPOSITO_REPLAN: {unidadId} intento {intento + 1}");
        }

        return ResultadoAccion.Fallido(
            "No se encontró una ruta libre hacia un Centro Urbano tras varios cambios del mapa. " +
            "La carga del Aldeano se conserva para poder reintentarla.");
    }

    private ResultadoAccion EjecutarHaciaObraConReplan(
        Guid unidadId,
        Guid obraId,
        ResultadoAproximacionConstruccion planInicial,
        TimeSpan retardoPaso,
        CancellationToken token)
    {
        const int maximoReplanes = 12;
        ResultadoAproximacionConstruccion planActual =
            planInicial;

        for (int intento = 1;
             intento <= maximoReplanes;
             intento++)
        {
            token.ThrowIfCancellationRequested();

            ResultadoAccion movimiento =
                EjecutarPasosConstruccion(
                    unidadId,
                    planActual.Pasos,
                    retardoPaso,
                    token);

            if (movimiento.Exito)
                return movimiento;

            if (intento == maximoReplanes)
                break;

            EsperarCesionPaso(
                unidadId,
                token,
                intento);

            planActual =
                estadoPartida.PrepararAproximacionConstruccion(
                    unidadId,
                    obraId,
                    true);

            if (!planActual.Exito)
            {
                if (planActual.Reintentable)
                {
                    EsperarAntesDeAplicar(
                        token,
                        retardoMovimiento);

                    continue;
                }

                return ResultadoAccion.Fallido(
                    planActual.Mensaje);
            }

            Console.WriteLine(
                $"CONSTRUCCION_REPLAN: {unidadId} intento {intento + 1}");
        }

        return ResultadoAccion.Fallido(
            "No se encontró una ruta libre hasta la obra tras varios cambios del mapa. " +
            "La obra se cancelará y su costo será reembolsado.");
    }

    private ResultadoAccion EjecutarPasosRecoleccion(
        Guid unidadId,
        IReadOnlyList<Coordenada> pasosPlanificados,
        TimeSpan retardoPaso,
        CancellationToken token)
    {
        var pasos =
            new Queue<Coordenada>(
                pasosPlanificados);

        while (pasos.Count > 0)
        {
            EsperarAntesDeAplicar(
                token,
                retardoPaso);

            Coordenada siguiente =
                pasos.Dequeue();

            ResultadoAccion resultado =
                AvanzarMovimientoConReintentos(
                    unidadId,
                    siguiente,
                    token);

            if (!resultado.Exito)
            {
                return resultado;
            }

            Console.WriteLine(
                $"RECOLECCION_MOVIMIENTO: {unidadId} -> " +
                $"({siguiente.X},{siguiente.Y})");
        }

        return ResultadoAccion.Exitoso(
            "Movimiento de recolección completado.");
    }


    private ResultadoAccion EjecutarPasosConstruccion(
        Guid unidadId,
        IReadOnlyList<Coordenada> pasosPlanificados,
        TimeSpan retardoPaso,
        CancellationToken token)
    {
        var pasos =
            new Queue<Coordenada>(
                pasosPlanificados);

        while (pasos.Count > 0)
        {
            EsperarAntesDeAplicar(
                token,
                retardoPaso);

            Coordenada siguiente =
                pasos.Dequeue();

            ResultadoAccion resultado =
                AvanzarMovimientoConReintentos(
                    unidadId,
                    siguiente,
                    token);

            if (!resultado.Exito)
                return resultado;

            Console.WriteLine(
                $"CONSTRUCCION_MOVIMIENTO: {unidadId} -> " +
                $"({siguiente.X},{siguiente.Y})");
        }

        return ResultadoAccion.Exitoso(
            "Aldeano posicionado junto a la obra.");
    }

    private ResultadoAccion AvanzarMovimientoConReintentos(
        Guid unidadId,
        Coordenada siguiente,
        CancellationToken token)
    {
        const int maximoIntentos = 3;

        ResultadoAccion ultimo =
            ResultadoAccion.Fallido(
                "No se pudo avanzar el movimiento.");

        for (int intento = 1;
             intento <= maximoIntentos;
             intento++)
        {
            token.ThrowIfCancellationRequested();

            ultimo =
                estadoPartida.AvanzarMovimiento(
                    unidadId,
                    siguiente);

            if (ultimo.Exito)
            {
                return ultimo;
            }

            if (intento < maximoIntentos)
            {
                EsperarCesionPaso(
                    unidadId,
                    token,
                    intento);
            }
        }

        return ResultadoAccion.Fallido(
            $"El paso está temporalmente bloqueado tras {maximoIntentos} intentos. " +
            ultimo.Mensaje);
    }


    private void EsperarCesionPaso(
        Guid unidadId,
        CancellationToken token,
        int intento)
    {
        if (retardoMovimiento <= TimeSpan.Zero)
            return;

        byte[] bytes =
            unidadId.ToByteArray();

        int marca =
            bytes[0] ^
            bytes[5] ^
            bytes[10] ^
            bytes[15];

        double baseMs =
            Math.Max(
                50d,
                Math.Min(
                    250d,
                    retardoMovimiento.TotalMilliseconds * 0.20d));

        double esperaMs =
            baseMs * (1 + (marca % 5)) +
            Math.Min(intento, 4) * 25d;

        EsperarAntesDeAplicar(
            token,
            TimeSpan.FromMilliseconds(
                esperaMs));
    }


    private static TimeSpan MultiplicarRetardo(
        TimeSpan baseTiempo,
        double factor)
    {
        if (baseTiempo <= TimeSpan.Zero)
            return TimeSpan.Zero;

        if (factor <= 0d ||
            double.IsNaN(factor) ||
            double.IsInfinity(factor))
        {
            throw new ArgumentOutOfRangeException(
                nameof(factor));
        }

        long ticks =
            (long)Math.Round(
                baseTiempo.Ticks *
                factor);

        return TimeSpan.FromTicks(
            Math.Max(1L, ticks));
    }


    private static TimeSpan DividirRetardo(
        TimeSpan total,
        int partes)
    {
        if (partes <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(partes));
        }

        if (total <= TimeSpan.Zero)
            return TimeSpan.Zero;

        long ticks =
            Math.Max(
                1L,
                total.Ticks / partes);

        return TimeSpan.FromTicks(ticks);
    }


    private static TimeSpan CalcularRetardoPasoMovimiento(
        TimeSpan retardoBase,
        double velocidadMovimiento)
    {
        if (retardoBase <= TimeSpan.Zero)
        {
            return TimeSpan.Zero;
        }

        if (velocidadMovimiento <= 0d ||
            double.IsNaN(velocidadMovimiento) ||
            double.IsInfinity(velocidadMovimiento))
        {
            throw new ArgumentOutOfRangeException(
                nameof(velocidadMovimiento));
        }

        long ticks =
            (long)Math.Round(
                retardoBase.Ticks /
                velocidadMovimiento);

        return TimeSpan.FromTicks(
            Math.Max(1L, ticks));
    }


    // ============================================================
    // ESPERA CANCELABLE
    // ============================================================

    private static void EsperarAntesDeAplicar(
        CancellationToken token,
        TimeSpan retardo)
    {
        token.ThrowIfCancellationRequested();

        if (retardo <= TimeSpan.Zero)
        {
            return;
        }

        if (token.WaitHandle.WaitOne(
            retardo))
        {
            token.ThrowIfCancellationRequested();
        }
    }


    // ============================================================
    // VALIDACIÓN DE RETARDOS
    // ============================================================

    private static void ValidarRetardo(
        TimeSpan retardo,
        string nombreParametro)
    {
        if (retardo < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nombreParametro);
        }
    }


    // ============================================================
    // COPIAS DE REQUESTS
    // ============================================================
    //
    // Los workers trabajan con copias de los datos recibidos.
    // Esto evita depender de objetos que podrían ser modificados
    // externamente mientras una Task se está ejecutando.
    // ============================================================

    private static EntrenarRequest? Copiar(
        EntrenarRequest? request)
    {
        if (request == null)
        {
            return null;
        }

        return new EntrenarRequest
        {
            TipoUnidad =
                request.TipoUnidad,

            EdificioOrigen =
                request.EdificioOrigen == null
                    ? null
                    : new CoordenadaRequest
                    {
                        X =
                            request.EdificioOrigen.X,

                        Y =
                            request.EdificioOrigen.Y
                    },

            Destino =
                request.Destino == null
                    ? null
                    : new CoordenadaRequest
                    {
                        X =
                            request.Destino.X,

                        Y =
                            request.Destino.Y
                    }
        };
    }


    private static ConstruirRequest? Copiar(
        ConstruirRequest? request)
    {
        if (request == null)
        {
            return null;
        }

        return new ConstruirRequest
        {
            AldeanoId =
                request.AldeanoId,

            TipoEdificio =
                request.TipoEdificio,

            Destino =
                request.Destino == null
                    ? null
                    : new CoordenadaRequest
                    {
                        X =
                            request.Destino.X,

                        Y =
                            request.Destino.Y
                    }
        };
    }


    private static RecolectarRequest? Copiar(
        RecolectarRequest? request)
    {
        if (request == null)
        {
            return null;
        }

        return new RecolectarRequest
        {
            AldeanoId =
                request.AldeanoId,

            Objetivo =
                request.Objetivo == null
                    ? null
                    : new CoordenadaRequest
                    {
                        X =
                            request.Objetivo.X,

                        Y =
                            request.Objetivo.Y
                    }
        };
    }


    private static MoverUnidadRequest? Copiar(
        MoverUnidadRequest? request)
    {
        if (request == null)
        {
            return null;
        }

        return new MoverUnidadRequest
        {
            UnidadId =
                request.UnidadId,

            Destino =
                request.Destino == null
                    ? null
                    : new CoordenadaRequest
                    {
                        X =
                            request.Destino.X,

                        Y =
                            request.Destino.Y
                    }
        };
    }


    private static AtacarRequest? Copiar(
        AtacarRequest? request)
    {
        if (request == null)
        {
            return null;
        }

        return new AtacarRequest
        {
            AtacanteId =
                request.AtacanteId,

            ObjetivoId =
                request.ObjetivoId
        };
    }
}