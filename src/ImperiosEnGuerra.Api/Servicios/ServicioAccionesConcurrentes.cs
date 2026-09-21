using System;
using System.Collections.Generic;
using System.Threading;
using ImperiosEnGuerra.Modelo.Unidades;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Servicios.Concurrencia;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Movimiento;
using ImperiosEnGuerra.Modelo.Recoleccion;
using ImperiosEnGuerra.Modelo.Recursos;

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


    // ============================================================
    // CONSTRUCTOR USADO POR LA API MEDIANTE INYECCIÓN DE DEPENDENCIAS
    // ============================================================
    //
    // Mantiene los tiempos de demostración actuales del proyecto:
    //
    // Movimiento:      1 segundo
    // Recolección:     2 segundos
    // Construcción:    7 segundos
    // Entrenamiento:   5 segundos
    // Ataque:          1 segundo
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
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(7),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(1))
    {
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
                    return ResultadoAccion.Fallido(
                        plan.Mensaje);
                }

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
                    return ResultadoAccion.Fallido(
                        "La unidad no está disponible.");
                }

                try
                {
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

                ResultadoAproximacionRecurso planInicial =
                    PrepararAproximacionRecursoConReintentos(
                        copia,
                        token);

                if (!planInicial.Exito ||
                    !planInicial.TipoRecurso.HasValue)
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

                TimeSpan retardoPaso =
                    CalcularRetardoPasoMovimiento(
                        retardoMovimiento,
                        aldeano.VelocidadMovimiento);

                int totalDepositado = 0;

                try
                {
                    ResultadoAccion movimientoInicial =
                        EjecutarPasosRecoleccion(
                            unidadId,
                            planInicial.Pasos,
                            retardoPaso,
                            token);

                    if (!movimientoInicial.Exito)
                    {
                        return movimientoInicial;
                    }

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
                                planInicial.TipoRecurso.Value);

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
                                EjecutarPasosRecoleccion(
                                    unidadId,
                                    depositoPlan.Pasos,
                                    retardoPaso,
                                    token);

                            if (!regreso.Exito)
                            {
                                return regreso;
                            }

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
                                objetivo))
                        {
                            return ResultadoAccion.Exitoso(
                                $"Recolección completada. Se depositaron {totalDepositado} de {planInicial.TipoRecurso.Value} y el nodo quedó agotado.");
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
                                    objetivo))
                            {
                                return ResultadoAccion.Exitoso(
                                    $"Recolección completada. Se depositaron {totalDepositado} de {planInicial.TipoRecurso.Value}.");
                            }

                            return ResultadoAccion.Fallido(
                                nuevoPlan.Mensaje);
                        }

                        ResultadoAccion regresoRecurso =
                            EjecutarPasosRecoleccion(
                                unidadId,
                                nuevoPlan.Pasos,
                                retardoPaso,
                                token);

                        if (!regresoRecurso.Exito)
                        {
                            return regresoRecurso;
                        }
                    }
                }
                finally
                {
                    estadoPartida.CompletarOrdenUnidad(
                        unidadId);
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
                Unidad? unidad = null;

                if (Guid.TryParse(
                    copia?.AldeanoId,
                    out Guid unidadId))
                {
                    unidad =
                        estadoPartida.ObtenerUnidad(
                            unidadId);
                }

                try
                {
                    EsperarAntesDeAplicar(
                        token,
                        retardoConstruccion);

                    var resultado =
                        estadoPartida.Construir(
                            copia);

                    if (resultado.Exito &&
                        unidad != null)
                    {
                        servicioOrdenes.Iniciar(
                            unidad,
                            TipoAccionJuego.Construir);
                    }

                    return resultado;
                }
                finally
                {
                    if (unidad != null)
                    {
                        servicioOrdenes.Completar(
                            unidad);
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
                CentroUrbano? centro = null;

                if (copia?.EdificioOrigen != null)
                {
                    centro =
                        estadoPartida.ObtenerCentroUrbano(
                            new Coordenada(
                                copia.EdificioOrigen.X,
                                copia.EdificioOrigen.Y));
                }

                try
                {
                    if (centro != null)
                    {
                        if (!centro.IniciarEntrenamiento(
                            copia?.TipoUnidad
                            ?? string.Empty))
                        {
                            return ResultadoAccion.Fallido(
                                "El Centro Urbano ya está entrenando.");
                        }
                    }

                    EsperarAntesDeAplicar(
                        token,
                        retardoEntrenamiento);

                    return estadoPartida.Entrenar(
                        copia);
                }
                finally
                {
                    if (centro != null)
                    {
                        centro.CompletarEntrenamiento();
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
                EsperarAntesDeAplicar(
                    token,
                    retardoAtaque);

                return estadoPartida.Atacar(
                    copia);
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
                estadoPartida.AvanzarMovimiento(
                    unidadId,
                    siguiente);

            if (!resultado.Exito)
            {
                return ResultadoAccion.Fallido(
                    resultado.Mensaje);
            }

            Console.WriteLine(
                $"RECOLECCION_MOVIMIENTO: {unidadId} -> " +
                $"({siguiente.X},{siguiente.Y})");
        }

        return ResultadoAccion.Exitoso(
            "Movimiento de recolección completado.");
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