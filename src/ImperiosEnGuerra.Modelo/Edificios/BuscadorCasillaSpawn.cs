using System;
using System.Linq;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Edificios
{
    /// <summary>
    /// Busca la casilla libre más cercana al edificio, evitando recursos,
    /// edificios y unidades de ambos jugadores.
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

            Mapa mapa =
                partida.JugadorHumano.Mapa;

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

                        if (!mapa.EstaDentroDeLimites(candidato))
                            continue;

                        Casilla casilla =
                            mapa.ObtenerCasilla(
                                candidato.X,
                                candidato.Y);

                        if (casilla == null ||
                            !casilla.EsTransitable ||
                            !mapa.PuedeColocar(candidato))
                        {
                            continue;
                        }

                        bool entidad =
                            partida.JugadorHumano.Unidades.Any(
                                u => Coincide(u.Coordenada, candidato))
                            ||
                            partida.JugadorMaquina.Unidades.Any(
                                u => Coincide(u.Coordenada, candidato))
                            ||
                            partida.JugadorHumano.Edificios.Any(
                                e => Coincide(e.Coordenada, candidato))
                            ||
                            partida.JugadorMaquina.Edificios.Any(
                                e => Coincide(e.Coordenada, candidato));

                        if (!entidad)
                            return candidato;
                    }
                }
            }

            return null;
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
