using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Servicios.Concurrencia;

namespace ImperiosEnGuerra.Api.Servicios;

public sealed class ServicioAccionesConcurrentes
{
    private readonly EstadoPartidaService estadoPartida;
    private readonly GestorProcesosConcurrentes gestorProcesos;
    private readonly TimeSpan retardoMovimiento;
    private readonly TimeSpan retardoRecoleccion;
    private readonly TimeSpan retardoConstruccion;
    private readonly TimeSpan retardoEntrenamiento;

    public ServicioAccionesConcurrentes(
        EstadoPartidaService estadoPartida,
        GestorProcesosConcurrentes gestorProcesos,
        TimeSpan retardoDemostracion)
        : this(
            estadoPartida,
            gestorProcesos,
            retardoDemostracion,
            retardoDemostracion,
            retardoDemostracion,
            retardoDemostracion)
    {
    }

    public ServicioAccionesConcurrentes(
        EstadoPartidaService estadoPartida,
        GestorProcesosConcurrentes gestorProcesos,
        TimeSpan retardoMovimiento,
        TimeSpan retardoRecoleccion,
        TimeSpan retardoConstruccion,
        TimeSpan retardoEntrenamiento)
    {
        this.estadoPartida =
            estadoPartida ?? throw new ArgumentNullException(nameof(estadoPartida));

        this.gestorProcesos =
            gestorProcesos ?? throw new ArgumentNullException(nameof(gestorProcesos));

        ValidarRetardo(retardoMovimiento, nameof(retardoMovimiento));
        ValidarRetardo(retardoRecoleccion, nameof(retardoRecoleccion));
        ValidarRetardo(retardoConstruccion, nameof(retardoConstruccion));
        ValidarRetardo(retardoEntrenamiento, nameof(retardoEntrenamiento));

        this.retardoMovimiento = retardoMovimiento;
        this.retardoRecoleccion = retardoRecoleccion;
        this.retardoConstruccion = retardoConstruccion;
        this.retardoEntrenamiento = retardoEntrenamiento;
    }

    public ProcesoConcurrente IniciarMovimiento(
        MoverUnidadRequest? request)
    {
        MoverUnidadRequest? copia = Copiar(request);

        return gestorProcesos.Iniciar(
            "MOVER",
            token =>
            {
                EsperarAntesDeAplicar(token, retardoMovimiento);
                return estadoPartida.MoverUnidad(copia);
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
                EsperarAntesDeAplicar(token, retardoRecoleccion);
                return estadoPartida.IniciarRecoleccion(copia);
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
                EsperarAntesDeAplicar(token, retardoConstruccion);
                return estadoPartida.Construir(copia);
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
                EsperarAntesDeAplicar(token, retardoEntrenamiento);
                return estadoPartida.Entrenar(copia);
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
}
