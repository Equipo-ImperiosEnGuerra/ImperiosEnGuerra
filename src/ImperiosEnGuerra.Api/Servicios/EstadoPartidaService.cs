using ImperiosEnGuerra.Modelo.Core;

namespace ImperiosEnGuerra.Api.Servicios;

public sealed class EstadoPartidaService
{
    private readonly object sincronizacion = new();
    private Partida? partidaActiva;

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