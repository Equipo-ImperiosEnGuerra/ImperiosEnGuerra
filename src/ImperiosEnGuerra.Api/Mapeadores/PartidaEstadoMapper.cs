using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Api.Mapeadores;

public static class PartidaEstadoMapper
{
    public static EstadoPartidaResponse Convertir(Partida partida)
    {
        ArgumentNullException.ThrowIfNull(partida);

        Mapa mapa = partida.JugadorHumano.Mapa;

        return new EstadoPartidaResponse
        {
            Estado = "activa",
            Mapa = new MapaEstadoResponse
            {
                Ancho = mapa.Ancho,
                Alto = mapa.Alto,
                Recursos = mapa.Recursos.Select(recurso => new RecursoEstadoResponse
                {
                    Tipo = recurso.Tipo.ToString(),
                    Coordenada = ConvertirCoordenada(recurso.Coordenada),
                    CantidadRestante = recurso.CantidadRestante
                }).ToList()
            },
            JugadorHumano = ConvertirJugador(partida.JugadorHumano),
            JugadorMaquina = ConvertirJugador(partida.JugadorMaquina)
        };
    }

    private static JugadorEstadoResponse ConvertirJugador(Jugador jugador)
    {
        return new JugadorEstadoResponse
        {
            Nombre = jugador.Nombre,
            Tipo = jugador.Tipo.ToString(),
            Recursos = new RecursosJugadorEstadoResponse
            {
                Oro = jugador.Recursos.ObtenerCantidad(TipoRecurso.Oro),
                Madera = jugador.Recursos.ObtenerCantidad(TipoRecurso.Madera),
                Comida = jugador.Recursos.ObtenerCantidad(TipoRecurso.Comida)
            },
            Edificios = jugador.Edificios.Select(edificio => new EdificioEstadoResponse
            {
                Tipo = edificio.GetType().Name,
                Coordenada = ConvertirCoordenada(edificio.Coordenada)
            }).ToList(),
            ObrasConstruccion = jugador.ObrasConstruccion.Select(obra => new ObraConstruccionEstadoResponse
            {
                Id = obra.Id.ToString("D"),
                Tipo = obra.TipoEdificio,
                Coordenada = ConvertirCoordenada(obra.Coordenada),
                Progreso = obra.Progreso
            }).ToList(),
            Unidades = jugador.Unidades.Select(unidad => new UnidadEstadoResponse
            {
                Id = unidad.Id.ToString("D"),
                Tipo = unidad.GetType().Name,
                Coordenada = unidad.Coordenada == null
                    ? null
                    : ConvertirCoordenada(unidad.Coordenada),
                Disponible = unidad.Disponible,
                Estado = unidad.Estado.ToString(),
                OrdenActiva = unidad.OrdenActiva?.ToString(),
                CapacidadCarga = unidad is Aldeano aldeano
                    ? aldeano.CapacidadCarga
                    : 0,
                CargaActual = unidad is Aldeano aldeanoCarga
                    ? aldeanoCarga.CargaActual
                    : 0,
                TipoCarga = unidad is Aldeano aldeanoTipo
                    ? aldeanoTipo.TipoCarga?.ToString()
                    : null
            }).ToList()
        };
    }

    private static CoordenadaEstadoResponse ConvertirCoordenada(Coordenada coordenada)
    {
        return new CoordenadaEstadoResponse
        {
            X = coordenada.X,
            Y = coordenada.Y
        };
    }
}
