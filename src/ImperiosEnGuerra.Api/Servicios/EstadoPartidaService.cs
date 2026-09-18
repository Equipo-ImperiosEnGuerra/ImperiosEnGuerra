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

    public ResultadoAccion Construir(ConstruirRequest? request)
{
    lock (sincronizacion)
    {
        if (partidaActiva == null)
            return ResultadoAccion.Fallido(
                "No hay una partida activa.");

        if (request == null)
            return ResultadoAccion.Fallido(
                "La solicitud de construcción es obligatoria.");

        if (!Guid.TryParse(request.AldeanoId, out Guid aldeanoId))
            return ResultadoAccion.Fallido(
                "El ID del Aldeano debe tener formato Guid válido.");

        if (request.Destino == null)
            return ResultadoAccion.Fallido(
                "La posición de construcción es obligatoria.");

        var solicitud = new SolicitudConstruccion(
            aldeanoId,
            request.TipoEdificio ?? string.Empty,
            PartidaRequestMapper.ConvertirCoordenada(request.Destino));

        return new OperacionConstruccion()
            .Ejecutar(partidaActiva, solicitud);
    }
}

    public ResultadoAccion Entrenar(EntrenarRequest? request)
{
    lock (sincronizacion)
    {
        if (partidaActiva == null)
            return ResultadoAccion.Fallido(
                "No hay una partida activa.");

        if (request == null)
            return ResultadoAccion.Fallido(
                "La solicitud de entrenamiento es obligatoria.");

        if (request.EdificioOrigen == null)
            return ResultadoAccion.Fallido(
                "El edificio de origen es obligatorio.");

        if (request.Destino == null)
            return ResultadoAccion.Fallido(
                "La posición de aparición es obligatoria.");

        var solicitud = new SolicitudEntrenamiento(
            PartidaRequestMapper.ConvertirCoordenada(
                request.EdificioOrigen),
            request.TipoUnidad ?? string.Empty,
            PartidaRequestMapper.ConvertirCoordenada(
                request.Destino));

        return new OperacionEntrenamiento()
            .Ejecutar(partidaActiva, solicitud);
    }
}

    public ResultadoAccion Atacar(AtacarRequest? request)
    {
        lock (sincronizacion)
        {
            if (partidaActiva == null)
                return ResultadoAccion.Fallido(
                    "No hay una partida activa.");

            if (request == null)
                return ResultadoAccion.Fallido(
                    "La solicitud de ataque es obligatoria.");

            if (!Guid.TryParse(request.AtacanteId, out Guid atacanteId))
                return ResultadoAccion.Fallido(
                    "El ID del atacante debe tener formato Guid válido.");

            if (!Guid.TryParse(request.ObjetivoId, out Guid objetivoId))
                return ResultadoAccion.Fallido(
                    "El ID del objetivo debe tener formato Guid válido.");

            var solicitud = new SolicitudAtaque(
                atacanteId,
                objetivoId);

            return new OperacionAtaque()
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
