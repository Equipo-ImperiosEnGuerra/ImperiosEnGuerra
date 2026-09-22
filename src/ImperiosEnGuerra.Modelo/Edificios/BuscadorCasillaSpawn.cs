using System;
using System.Collections.Generic;
using System.Linq;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Edificios
{
    /// <summary>
    /// Busca una casilla libre próxima al edificio, priorizando posiciones
    /// interiores con varias salidas ortogonales para evitar unidades atrapadas.
    /// </summary>
    public sealed class BuscadorCasillaSpawn
    {
        private static readonly (int X, int Y)[] Direcciones =
        {
            (1, 0),
            (-1, 0),
            (0, 1),
            (0, -1)
        };

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
                var candidatas =
                    new List<(Coordenada Posicion, int Salidas, bool Borde)>();

                for (int x = 0;
                     x < mapa.Ancho;
                     x++)
                {
                    for (int y = 0;
                         y < mapa.Alto;
                         y++)
                    {
                        if (Math.Abs(x - edificio.X) +
                            Math.Abs(y - edificio.Y) != distancia)
                        {
                            continue;
                        }

                        var candidato =
                            new Coordenada(x, y);

                        if (!EsLibre(
                                partida,
                                mapa,
                                candidato))
                        {
                            continue;
                        }

                        int salidas =
                            ContarSalidasLibres(
                                partida,
                                mapa,
                                candidato);

                        if (salidas <= 0)
                            continue;

                        bool borde =
                            x == 0 ||
                            y == 0 ||
                            x == mapa.Ancho - 1 ||
                            y == mapa.Alto - 1;

                        candidatas.Add(
                            (candidato, salidas, borde));
                    }
                }

                if (candidatas.Count == 0)
                    continue;

                return candidatas
                    .OrderBy(c => c.Borde)
                    .ThenByDescending(c => c.Salidas)
                    .ThenBy(c => c.Posicion.X)
                    .ThenBy(c => c.Posicion.Y)
                    .First()
                    .Posicion;
            }

            return null;
        }

        private static int ContarSalidasLibres(
            Partida partida,
            Mapa mapa,
            Coordenada posicion)
        {
            int salidas = 0;

            foreach ((int X, int Y) direccion
                in Direcciones)
            {
                var vecina =
                    new Coordenada(
                        posicion.X + direccion.X,
                        posicion.Y + direccion.Y);

                if (EsLibre(
                        partida,
                        mapa,
                        vecina))
                {
                    salidas++;
                }
            }

            return salidas;
        }

        private static bool EsLibre(
            Partida partida,
            Mapa mapa,
            Coordenada candidato)
        {
            if (!mapa.EstaDentroDeLimites(
                    candidato))
            {
                return false;
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
                return false;
            }

            return !TieneEntidadEnMapa(
                       partida.JugadorHumano,
                       mapa,
                       candidato)
                   &&
                   !TieneEntidadEnMapa(
                       partida.JugadorMaquina,
                       mapa,
                       candidato);
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
