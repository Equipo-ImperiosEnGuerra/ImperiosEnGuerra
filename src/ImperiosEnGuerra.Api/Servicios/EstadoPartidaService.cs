using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Api.Mapeadores;

namespace ImperiosEnGuerra.Api.Servicios;

public sealed class EstadoPartidaService
{
    private readonly object sincronizacion = new();
    private Partida? partidaActiva;

    public ResultadoAccion MoverUnidad(MoverUnidadRequest? request)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return ResultadoAccion.Fallido("No hay una partida activa.");
            if (request == null)
                return ResultadoAccion.Fallido("La solicitud de movimiento es obligatoria.");
            if (!Guid.TryParse(request.UnidadId, out Guid unidadId))
                return ResultadoAccion.Fallido("El ID de la unidad debe tener formato Guid válido.");
            if (request.Destino == null)
                return ResultadoAccion.Fallido("El destino es obligatorio.");

            var solicitud = new SolicitudMovimiento(unidadId,
                PartidaRequestMapper.ConvertirCoordenada(request.Destino));
            return new OperacionMovimiento().Ejecutar(partidaActiva, solicitud);
        }
    }
    public ResultadoAccion IniciarRecoleccion(RecolectarRequest? request)
    {
        lock (sincronizacion)
        {
        if (partidaActiva == null)
            return ResultadoAccion.Fallido(
                "No hay una partida activa.");

        if (request == null)
            return ResultadoAccion.Fallido(
                "La solicitud de recolección es obligatoria.");

        if (!Guid.TryParse(request.AldeanoId, out Guid aldeanoId))
            return ResultadoAccion.Fallido(
                "El ID del Aldeano debe tener formato Guid válido.");

        if (request.Objetivo == null)
            return ResultadoAccion.Fallido(
                "El objetivo de recolección es obligatorio.");

        var solicitud = new SolicitudRecoleccion(
            aldeanoId,
            PartidaRequestMapper.ConvertirCoordenada(request.Objetivo));

        return new OperacionRecoleccion()
            .Ejecutar(partidaActiva, solicitud);
        }
    }

    public EstadoPartidaResponse? ObtenerEstado()
    {
        lock (sincronizacion)
        {
            return partidaActiva == null ? null : PartidaEstadoMapper.Convertir(partidaActiva);
        }
    }

    public void EstablecerPartida(Partida partida)
    {
        ArgumentNullException.ThrowIfNull(partida);

        lock (sincronizacion)
        {
            partidaActiva = partida;
        }
    }

    public Partida? ObtenerPartida()
    {
        lock (sincronizacion)
        {
            return partidaActiva;
        }
    }

    public bool HayPartidaActiva()
    {
        lock (sincronizacion)
        {
            return partidaActiva != null;
        }
    }
}
