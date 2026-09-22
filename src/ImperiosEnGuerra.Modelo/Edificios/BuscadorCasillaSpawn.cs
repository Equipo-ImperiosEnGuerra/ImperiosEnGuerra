using System;
using System.Linq;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Edificios
{
    /// <summary>
    /// Busca la casilla libre más cercana al edificio, evitando recursos,
    /// edificios y unidades que comparten el mismo mapa.
    /// </summary>
    public sealed class BuscadorCasillaSpawn
    {
        public Coordenada Buscar(
            Partida partida,
            Coordenada edificio)
        {
            if (partida == null)
                throw new ArgumentNullException(nameof(partida));

            if (edificio == null)
                throw new ArgumentNullException(nameof(edificio));

            Jugador propietario =
                partida.BuscarJugadorPorEdificio(
                    edificio);

            return propietario == null
                ? null
                : Buscar(
                    partida,
                    propietario.Tipo,
                    edificio);
        }

        public Coordenada Buscar(
            Partida partida,
            TipoJugador propietarioTipo,
            Coordenada edificio)
        {
            if (partida == null)
                throw new ArgumentNullException(nameof(partida));

            if (edificio == null)
                throw new ArgumentNullException(nameof(edificio));

            Jugador propietario =
                partida.ObtenerJugador(
                    propietarioTipo);

            if (propietario == null)
                return null;

            Mapa mapa =
                propietario.Mapa;

            for (int distancia = 1;
                 distancia <= mapa.Ancho + mapa.Alto;
                 distancia++)
            {
                for (int dx = -distancia;
                     dx <= distancia;
                     dx++)
                {
                    int dy =
                        distancia -
                        Math.Abs(dx);

                    foreach (int signo in dy == 0
                                 ? new[] { 1 }
                                 : new[] { 1, -1 })
                    {
                        Coordenada candidato =
                            new Coordenada(
                                edificio.X + dx,
                                edificio.Y + dy * signo);

                        if (!mapa.EstaDentroDeLimites(
                                candidato))
                        {
                            continue;
                        }

                        Casilla casilla =
                            mapa.ObtenerCasilla(
                                candidato.X,
                                candidato.Y);

                        if (casilla == null ||
                            !casilla.EsTransitable ||
                            !mapa.PuedeColocar(
                                candidato))
                        {
                            continue;
                        }

                        bool entidad =
                            TieneEntidadEnMapa(
                                partida.JugadorHumano,
                                mapa,
                                candidato)
                            ||
                            TieneEntidadEnMapa(
                                partida.JugadorMaquina,
                                mapa,
                                candidato);

                        if (!entidad)
                            return candidato;
                    }
                }
            }

            return null;
        }

        private static bool TieneEntidadEnMapa(
            Jugador jugador,
            Mapa mapa,
            Coordenada candidato)
        {
            if (!ReferenceEquals(
                    jugador.Mapa,
                    mapa))
            {
                return false;
            }

            return jugador.Unidades.Any(
                       u => Coincide(
                           u.Coordenada,
                           candidato))
                   ||
                   jugador.Edificios.Any(
                       e => Coincide(
                           e.Coordenada,
                           candidato));
        }

        private static bool Coincide(
            Coordenada a,
            Coordenada b)
        {
            return a != null &&
                   b != null &&
                   a.X == b.X &&
                   a.Y == b.Y;
        }
    }
}
