using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;

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
                    Coordenada = ConvertirCoordenada(recurso.Coordenada)
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
            Unidades = jugador.Unidades.Select(unidad => new UnidadEstadoResponse
            {
                Tipo = unidad.GetType().Name,
                Coordenada = unidad.Coordenada == null
                    ? null
                    : ConvertirCoordenada(unidad.Coordenada),
                Disponible = unidad.Disponible
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
