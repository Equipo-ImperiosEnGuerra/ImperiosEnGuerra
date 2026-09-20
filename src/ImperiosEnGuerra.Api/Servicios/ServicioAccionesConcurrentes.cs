using ImperiosEnGuerra.Modelo.Unidades;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Servicios.Concurrencia;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;

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

    public ServicioAccionesConcurrentes(
        EstadoPartidaService estadoPartida,
        GestorProcesosConcurrentes gestorProcesos,
        ServicioOrdenesUnidad servicioOrdenes,
        TimeSpan retardoMovimiento,
        TimeSpan retardoRecoleccion,
        TimeSpan retardoConstruccion,
        TimeSpan retardoEntrenamiento,
        TimeSpan retardoAtaque )
    {
        this.estadoPartida =
            estadoPartida ?? throw new ArgumentNullException(nameof(estadoPartida));

        this.gestorProcesos =
            gestorProcesos ?? throw new ArgumentNullException(nameof(gestorProcesos));
        
        this.servicioOrdenes =
            servicioOrdenes ?? throw new ArgumentNullException(nameof(servicioOrdenes));

        ValidarRetardo(retardoMovimiento, nameof(retardoMovimiento));
        ValidarRetardo(retardoRecoleccion, nameof(retardoRecoleccion));
        ValidarRetardo(retardoConstruccion, nameof(retardoConstruccion));
        ValidarRetardo(retardoEntrenamiento, nameof(retardoEntrenamiento));
        ValidarRetardo(retardoAtaque, nameof(retardoAtaque));

        this.retardoMovimiento = retardoMovimiento;
        this.retardoRecoleccion = retardoRecoleccion;
        this.retardoConstruccion = retardoConstruccion;
        this.retardoEntrenamiento = retardoEntrenamiento;
        this.retardoAtaque = retardoAtaque;
    }

    public ProcesoConcurrente IniciarMovimiento(
        MoverUnidadRequest? request)
    {
        MoverUnidadRequest? copia = Copiar(request);

        return gestorProcesos.Iniciar(
            "MOVER",
            token =>
            {
            Unidad? unidad = null;

            if (Guid.TryParse(copia?.UnidadId, out Guid unidadId))
            {
                unidad = estadoPartida.ObtenerUnidad(unidadId);
            }

            try
            {
                EsperarAntesDeAplicar(token, retardoMovimiento);

                var resultado = estadoPartida.MoverUnidad(copia);

                if (resultado.Exito && unidad != null)
                {
                    servicioOrdenes.Iniciar(
                        unidad,
                        TipoAccionJuego.Mover);
                }

                Console.WriteLine(
                    $"MOVIMIENTO: {resultado.Mensaje}");

                return resultado;
            }
            finally
            {
                if (unidad != null)
                {
                    servicioOrdenes.Completar(unidad);
                }
            }
        });
}

    public ProcesoConcurrente IniciarRecoleccion(
        RecolectarRequest? request)
    {
        RecolectarRequest? copia = Copiar(request);

        return gestorProcesos.Iniciar(
            "RECOLECTAR",
            token =>
            {
                Unidad? unidad = null;

                if (Guid.TryParse(copia?.AldeanoId, out Guid unidadId))
                {
                    unidad = estadoPartida.ObtenerUnidad(unidadId);
                }

                try
                {
                    EsperarAntesDeAplicar(token, retardoRecoleccion);

                    var resultado = estadoPartida.IniciarRecoleccion(copia);

                    if (resultado.Exito && unidad != null)
                    {
                        servicioOrdenes.Iniciar(
                            unidad,
                            TipoAccionJuego.Recolectar);
                    }

                    return resultado;
                }
                finally
                {
                    if (unidad != null)
                    {
                        servicioOrdenes.Completar(unidad);
                    }
                }
            });
    }

    public ProcesoConcurrente IniciarConstruccion(
        ConstruirRequest? request)
    {
        ConstruirRequest? copia = Copiar(request);

        return gestorProcesos.Iniciar(
            "CONSTRUIR",
            token =>
            {
                Unidad? unidad = null;

                if (Guid.TryParse(copia?.AldeanoId, out Guid unidadId))
                {
                    unidad = estadoPartida.ObtenerUnidad(unidadId);
                }

                try
                {
                    EsperarAntesDeAplicar(token, retardoConstruccion);

                    var resultado = estadoPartida.Construir(copia);

                    if (resultado.Exito && unidad != null)
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
                        servicioOrdenes.Completar(unidad);
                    }
                }
            });
    }

    public ProcesoConcurrente IniciarEntrenamiento(
        EntrenarRequest? request)
    {
        EntrenarRequest? copia = Copiar(request);

        return gestorProcesos.Iniciar(
            "ENTRENAR",
            token =>
            {
                CentroUrbano? centro = null;

                if (copia?.EdificioOrigen != null)
                {
                    centro = estadoPartida.ObtenerCentroUrbano(
                        new Coordenada(
                            copia.EdificioOrigen.X,
                            copia.EdificioOrigen.Y));
                }

                try
                {
                    if (centro != null)
                    {
                        if (!centro.IniciarEntrenamiento(
                            copia?.TipoUnidad ?? string.Empty))
                        {
                            return ResultadoAccion.Fallido(
                                "El Centro Urbano ya está entrenando.");
                        }
                    }

                    EsperarAntesDeAplicar(token, retardoEntrenamiento);

                    return estadoPartida.Entrenar(copia);
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

    public ProcesoConcurrente IniciarAtaque(
        AtacarRequest? request)
    {
        AtacarRequest? copia = Copiar(request);

        return gestorProcesos.Iniciar(
            "ATACAR",
            token =>
            {
                EsperarAntesDeAplicar(token, retardoAtaque);

                return estadoPartida.Atacar(copia);
            });
    }
    public bool Cancelar(Guid procesoId)
    {
        return gestorProcesos.Cancelar(procesoId);
    }

    public void CancelarTodos()
    {
        gestorProcesos.CancelarTodos();
    }

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
        return gestorProcesos.IntentarObtenerResultado(out resultado);
    }

    public int ProcesosActivos => gestorProcesos.ProcesosActivos;

    private static void EsperarAntesDeAplicar(
        CancellationToken token,
        TimeSpan retardo)
    {
        token.ThrowIfCancellationRequested();

        if (retardo <= TimeSpan.Zero)
            return;

        if (token.WaitHandle.WaitOne(retardo))
            token.ThrowIfCancellationRequested();
    }

    private static void ValidarRetardo(
        TimeSpan retardo,
        string nombreParametro)
    {
        if (retardo < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nombreParametro);
    }

    private static EntrenarRequest? Copiar(
        EntrenarRequest? request)
    {
        if (request == null)
            return null;

        return new EntrenarRequest
        {
            TipoUnidad = request.TipoUnidad,
            EdificioOrigen = request.EdificioOrigen == null
                ? null
                : new CoordenadaRequest
                {
                    X = request.EdificioOrigen.X,
                    Y = request.EdificioOrigen.Y
                },
            Destino = request.Destino == null
                ? null
                : new CoordenadaRequest
                {
                    X = request.Destino.X,
                    Y = request.Destino.Y
                }
        };
    }

    private static ConstruirRequest? Copiar(
        ConstruirRequest? request)
    {
        if (request == null)
            return null;

        return new ConstruirRequest
        {
            AldeanoId = request.AldeanoId,
            TipoEdificio = request.TipoEdificio,
            Destino = request.Destino == null
                ? null
                : new CoordenadaRequest
                {
                    X = request.Destino.X,
                    Y = request.Destino.Y
                }
        };
    }

    private static RecolectarRequest? Copiar(
        RecolectarRequest? request)
    {
        if (request == null)
            return null;

        return new RecolectarRequest
        {
            AldeanoId = request.AldeanoId,
            Objetivo = request.Objetivo == null
                ? null
                : new CoordenadaRequest
                {
                    X = request.Objetivo.X,
                    Y = request.Objetivo.Y
                }
        };
    }

    private static MoverUnidadRequest? Copiar(
        MoverUnidadRequest? request)
    {
        if (request == null)
            return null;

        return new MoverUnidadRequest
        {
            UnidadId = request.UnidadId,
            Destino = request.Destino == null
                ? null
                : new CoordenadaRequest
                {
                    X = request.Destino.X,
                    Y = request.Destino.Y
                }
        };
    }

    private static AtacarRequest? Copiar(
        AtacarRequest? request)
    {
        if (request == null)
            return null;

        return new AtacarRequest
        {
            AtacanteId = request.AtacanteId,
            ObjetivoId = request.ObjetivoId
        };
    }
}
