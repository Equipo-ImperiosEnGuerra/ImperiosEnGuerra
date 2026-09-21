using System.IO;
using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Api.Mapeadores;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Unidades;
using ImperiosEnGuerra.Servicios;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Movimiento;
using ImperiosEnGuerra.Modelo.Recoleccion;
using System.Linq; // para usar FirstOrDefault()

namespace ImperiosEnGuerra.Api.Servicios;

public sealed class EstadoPartidaService
{
    private readonly object sincronizacion = new();
    private readonly ServicioArchivos? servicioArchivos;
    private Partida? partidaActiva;

    public EstadoPartidaService()
    {
    }

    public EstadoPartidaService(ServicioArchivos servicioArchivos)
    {
        this.servicioArchivos =
            servicioArchivos ?? throw new ArgumentNullException(nameof(servicioArchivos));
    }

    public ResultadoAccion MoverUnidad(MoverUnidadRequest? request)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return RegistrarResultado(
                    "MOVER",
                    ResultadoAccion.Fallido("No hay una partida activa."));

            if (request == null)
                return RegistrarResultado(
                    "MOVER",
                    ResultadoAccion.Fallido("La solicitud de movimiento es obligatoria."));

            if (!Guid.TryParse(request.UnidadId, out Guid unidadId))
                return RegistrarResultado(
                    "MOVER",
                    ResultadoAccion.Fallido("El ID de la unidad debe tener formato Guid válido."));

            if (request.Destino == null)
                return RegistrarResultado(
                    "MOVER",
                    ResultadoAccion.Fallido("El destino es obligatorio."));

            var solicitud = new SolicitudMovimiento(
                unidadId,
                PartidaRequestMapper.ConvertirCoordenada(request.Destino));

            return RegistrarResultado(
                "MOVER",
                new OperacionMovimiento().Ejecutar(partidaActiva, solicitud));
        }
    }

    public ResultadoPlanMovimiento PrepararMovimientoProgresivo(
        MoverUnidadRequest? request,
        bool permitirOrdenMovimientoActiva = false)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return ResultadoPlanMovimiento.Fallido(
                    "No hay una partida activa.");

            if (request == null)
                return ResultadoPlanMovimiento.Fallido(
                    "La solicitud de movimiento es obligatoria.");

            if (!Guid.TryParse(
                    request.UnidadId,
                    out Guid unidadId))
            {
                return ResultadoPlanMovimiento.Fallido(
                    "El ID de la unidad debe tener formato Guid válido.");
            }

            if (request.Destino == null)
                return ResultadoPlanMovimiento.Fallido(
                    "El destino es obligatorio.");

            var solicitud =
                new SolicitudMovimiento(
                    unidadId,
                    PartidaRequestMapper.ConvertirCoordenada(
                        request.Destino));

            return new PlanificadorMovimiento()
                .Preparar(
                    partidaActiva,
                    solicitud,
                    permitirOrdenMovimientoActiva);
        }
    }

    public bool IntentarIniciarOrdenUnidad(
        Guid unidadId,
        TipoAccionJuego tipo)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return false;

            Unidad? unidad =
                partidaActiva.JugadorHumano.Unidades
                    .FirstOrDefault(
                        u => u.Id == unidadId);

            return unidad != null &&
                   unidad.IntentarIniciarOrden(tipo);
        }
    }

    public void CompletarOrdenUnidad(
        Guid unidadId)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return;

            Unidad? unidad =
                partidaActiva.JugadorHumano.Unidades
                    .FirstOrDefault(
                        u => u.Id == unidadId);

            unidad?.CompletarOrden();
        }
    }

    public ResultadoAccion AvanzarMovimiento(
        Guid unidadId,
        Coordenada siguiente)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return ResultadoAccion.Fallido(
                    "No hay una partida activa.");

            return new OperacionPasoMovimiento()
                .Ejecutar(
                    partidaActiva,
                    unidadId,
                    siguiente);
        }
    }

    public ResultadoAproximacionRecurso PrepararAproximacionRecurso(
        RecolectarRequest? request)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
            {
                return ResultadoAproximacionRecurso.Fallido(
                    "No hay una partida activa.");
            }

            if (request == null)
            {
                return ResultadoAproximacionRecurso.Fallido(
                    "La solicitud de recolección es obligatoria.");
            }

            if (!Guid.TryParse(
                    request.AldeanoId,
                    out Guid aldeanoId))
            {
                return ResultadoAproximacionRecurso.Fallido(
                    "El ID del Aldeano debe tener formato Guid válido.");
            }

            if (request.Objetivo == null)
            {
                return ResultadoAproximacionRecurso.Fallido(
                    "El objetivo de recolección es obligatorio.");
            }

            var solicitud =
                new SolicitudRecoleccion(
                    aldeanoId,
                    PartidaRequestMapper.ConvertirCoordenada(
                        request.Objetivo));

            return new PlanificadorAproximacionRecurso()
                .Preparar(
                    partidaActiva,
                    solicitud);
        }
    }

    public ResultadoAccion IniciarRecoleccion(RecolectarRequest? request)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return RegistrarResultado(
                    "RECOLECTAR",
                    ResultadoAccion.Fallido("No hay una partida activa."));

            if (request == null)
                return RegistrarResultado(
                    "RECOLECTAR",
                    ResultadoAccion.Fallido("La solicitud de recolección es obligatoria."));

            if (!Guid.TryParse(request.AldeanoId, out Guid aldeanoId))
                return RegistrarResultado(
                    "RECOLECTAR",
                    ResultadoAccion.Fallido("El ID del Aldeano debe tener formato Guid válido."));

            if (request.Objetivo == null)
                return RegistrarResultado(
                    "RECOLECTAR",
                    ResultadoAccion.Fallido("El objetivo de recolección es obligatorio."));

            var solicitud = new SolicitudRecoleccion(
                aldeanoId,
                PartidaRequestMapper.ConvertirCoordenada(request.Objetivo));

            return RegistrarResultado(
                "RECOLECTAR",
                new OperacionRecoleccion().Ejecutar(partidaActiva, solicitud));
        }
    }

    public ResultadoAccion Construir(ConstruirRequest? request)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return RegistrarResultado(
                    "CONSTRUIR",
                    ResultadoAccion.Fallido("No hay una partida activa."));

            if (request == null)
                return RegistrarResultado(
                    "CONSTRUIR",
                    ResultadoAccion.Fallido("La solicitud de construcción es obligatoria."));

            if (!Guid.TryParse(request.AldeanoId, out Guid aldeanoId))
                return RegistrarResultado(
                    "CONSTRUIR",
                    ResultadoAccion.Fallido("El ID del Aldeano debe tener formato Guid válido."));

            if (request.Destino == null)
                return RegistrarResultado(
                    "CONSTRUIR",
                    ResultadoAccion.Fallido("La posición de construcción es obligatoria."));

            var solicitud = new SolicitudConstruccion(
                aldeanoId,
                request.TipoEdificio ?? string.Empty,
                PartidaRequestMapper.ConvertirCoordenada(request.Destino));

            return RegistrarResultado(
                "CONSTRUIR",
                new OperacionConstruccion().Ejecutar(partidaActiva, solicitud));
        }
    }

    public ResultadoAccion Entrenar(EntrenarRequest? request)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return RegistrarResultado(
                    "ENTRENAR",
                    ResultadoAccion.Fallido("No hay una partida activa."));

            if (request == null)
                return RegistrarResultado(
                    "ENTRENAR",
                    ResultadoAccion.Fallido("La solicitud de entrenamiento es obligatoria."));

            if (request.EdificioOrigen == null)
                return RegistrarResultado(
                    "ENTRENAR",
                    ResultadoAccion.Fallido("El edificio de origen es obligatorio."));

            if (request.Destino == null)
                return RegistrarResultado(
                    "ENTRENAR",
                    ResultadoAccion.Fallido("La posición de aparición es obligatoria."));

            var solicitud = new SolicitudEntrenamiento(
                PartidaRequestMapper.ConvertirCoordenada(request.EdificioOrigen),
                request.TipoUnidad ?? string.Empty,
                PartidaRequestMapper.ConvertirCoordenada(request.Destino));

            return RegistrarResultado(
                "ENTRENAR",
                new OperacionEntrenamiento().Ejecutar(partidaActiva, solicitud));
        }
    }

    public ResultadoAccion Atacar(AtacarRequest? request)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return RegistrarResultado(
                    "ATACAR",
                    ResultadoAccion.Fallido("No hay una partida activa."));

            if (request == null)
                return RegistrarResultado(
                    "ATACAR",
                    ResultadoAccion.Fallido("La solicitud de ataque es obligatoria."));

            if (!Guid.TryParse(request.AtacanteId, out Guid atacanteId))
                return RegistrarResultado(
                    "ATACAR",
                    ResultadoAccion.Fallido("El ID del atacante debe tener formato Guid válido."));

            if (!Guid.TryParse(request.ObjetivoId, out Guid objetivoId))
                return RegistrarResultado(
                    "ATACAR",
                    ResultadoAccion.Fallido("El ID del objetivo debe tener formato Guid válido."));

            var solicitud = new SolicitudAtaque(
                atacanteId,
                objetivoId);

            return RegistrarResultado(
                "ATACAR",
                new OperacionAtaque().Ejecutar(partidaActiva, solicitud));
        }
    }

    public EstadoPartidaResponse? ObtenerEstado()
    {
        lock (sincronizacion)
        {
            return partidaActiva == null
                ? null
                : PartidaEstadoMapper.Convertir(partidaActiva);
        }
    }

    public void EstablecerPartida(Partida partida)
    {
        ArgumentNullException.ThrowIfNull(partida);

        lock (sincronizacion)
        {
            partidaActiva = partida;
            RegistrarEventoSeguro("PARTIDA|EXITO|Partida establecida.");
        }
    }

    public Partida? ObtenerPartida()
    {
        lock (sincronizacion)
        {
            return partidaActiva;
        }
    }
    public Unidad? ObtenerUnidad(Guid id)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return null;

            return partidaActiva.JugadorHumano.Unidades
                .FirstOrDefault(u => u.Id == id);
        }
    }

    public CentroUrbano? ObtenerCentroUrbano(Coordenada coordenada)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null || coordenada == null)
                return null;

            return partidaActiva.JugadorHumano.Edificios
                .OfType<CentroUrbano>()
                .FirstOrDefault(e =>
                    e.Coordenada.X == coordenada.X &&
                    e.Coordenada.Y == coordenada.Y);
        }
    }
    public bool HayPartidaActiva()
    {
        lock (sincronizacion)
        {
            return partidaActiva != null;
        }
    }

    private ResultadoAccion RegistrarResultado(
        string accion,
        ResultadoAccion resultado)
    {
        string estado = resultado.Exito ? "EXITO" : "RECHAZADO";
        RegistrarEventoSeguro($"{accion}|{estado}|{resultado.Mensaje}");
        return resultado;
    }

    private void RegistrarEventoSeguro(string contenido)
    {
        if (servicioArchivos == null)
            return;

        try
        {
            servicioArchivos.RegistrarEvento(contenido);
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine(
                $"No se pudo escribir log_partida.txt: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.Error.WriteLine(
                $"No se pudo escribir log_partida.txt: {ex.Message}");
        }
    }
}

