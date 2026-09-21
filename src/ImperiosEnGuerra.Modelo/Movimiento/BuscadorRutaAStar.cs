using System;
using System.Collections.Generic;
using System.Linq;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Movimiento
{
    public sealed class BuscadorRutaAStar
    {
        private static readonly (int X, int Y)[] Direcciones =
        {
            (1, 0),
            (-1, 0),
            (0, 1),
            (0, -1)
        };

        public ResultadoRuta Buscar(
            Mapa mapa,
            Coordenada origen,
            Coordenada destino)
        {
            if (mapa == null)
            {
                throw new ArgumentNullException(nameof(mapa));
            }

            if (origen == null)
            {
                throw new ArgumentNullException(nameof(origen));
            }

            if (destino == null)
            {
                throw new ArgumentNullException(nameof(destino));
            }

            if (!mapa.EstaDentroDeLimites(origen) ||
                !mapa.EstaDentroDeLimites(destino))
            {
                return ResultadoRuta.Imposible();
            }

            if (Coincide(origen, destino))
            {
                return ResultadoRuta.Exitosa(
                    Array.Empty<Coordenada>());
            }

            if (!EsTransitable(mapa, destino))
            {
                return ResultadoRuta.Imposible();
            }

            var abiertos =
                new List<(int X, int Y)>
                {
                    (origen.X, origen.Y)
                };

            var cerrados =
                new HashSet<(int X, int Y)>();

            var costoDesdeOrigen =
                new Dictionary<(int X, int Y), int>
                {
                    [(origen.X, origen.Y)] = 0
                };

            var anterior =
                new Dictionary<
                    (int X, int Y),
                    (int X, int Y)>();

            while (abiertos.Count > 0)
            {
                (int X, int Y) actual =
                    abiertos
                        .OrderBy(posicion =>
                            costoDesdeOrigen[posicion] +
                            Heuristica(
                                posicion.X,
                                posicion.Y,
                                destino.X,
                                destino.Y))
                        .ThenBy(posicion =>
                            Heuristica(
                                posicion.X,
                                posicion.Y,
                                destino.X,
                                destino.Y))
                        .First();

                abiertos.Remove(actual);

                if (actual.X == destino.X &&
                    actual.Y == destino.Y)
                {
                    return ResultadoRuta.Exitosa(
                        ReconstruirRuta(
                            anterior,
                            origen,
                            destino));
                }

                cerrados.Add(actual);

                foreach ((int X, int Y) direccion
                    in Direcciones)
                {
                    int vecinoX =
                        actual.X + direccion.X;

                    int vecinoY =
                        actual.Y + direccion.Y;

                    var vecino =
                        (X: vecinoX, Y: vecinoY);

                    if (cerrados.Contains(vecino))
                    {
                        continue;
                    }

                    Coordenada coordenadaVecino =
                        new Coordenada(
                            vecinoX,
                            vecinoY);

                    if (!mapa.EstaDentroDeLimites(
                        coordenadaVecino))
                    {
                        continue;
                    }

                    if (!EsTransitable(
                        mapa,
                        coordenadaVecino))
                    {
                        continue;
                    }

                    int costoTentativo =
                        costoDesdeOrigen[actual] + 1;

                    if (!costoDesdeOrigen.TryGetValue(
                            vecino,
                            out int costoConocido) ||
                        costoTentativo < costoConocido)
                    {
                        anterior[vecino] = actual;

                        costoDesdeOrigen[vecino] =
                            costoTentativo;

                        if (!abiertos.Contains(vecino))
                        {
                            abiertos.Add(vecino);
                        }
                    }
                }
            }

            return ResultadoRuta.Imposible();
        }

        private static bool EsTransitable(
            Mapa mapa,
            Coordenada coordenada)
        {
            Casilla casilla =
                mapa.ObtenerCasilla(
                    coordenada.X,
                    coordenada.Y);

            if (casilla == null)
            {
                return false;
            }

            if (!casilla.EsTransitable)
            {
                return false;
            }

            if (casilla.EstaOcupada)
            {
                return false;
            }

            if (mapa.ObtenerRecursoEn(
                    coordenada) != null)
            {
                return false;
            }

            return true;
        }

        private static int Heuristica(
            int x,
            int y,
            int destinoX,
            int destinoY)
        {
            return Math.Abs(destinoX - x) +
                   Math.Abs(destinoY - y);
        }

        private static IReadOnlyList<Coordenada>
            ReconstruirRuta(
                Dictionary<
                    (int X, int Y),
                    (int X, int Y)> anterior,
                Coordenada origen,
                Coordenada destino)
        {
            var ruta =
                new List<Coordenada>();

            var actual =
                (X: destino.X, Y: destino.Y);

            var inicio =
                (X: origen.X, Y: origen.Y);

            while (actual != inicio)
            {
                ruta.Add(
                    new Coordenada(
                        actual.X,
                        actual.Y));

                if (!anterior.TryGetValue(
                        actual,
                        out actual))
                {
                    return Array.Empty<Coordenada>();
                }
            }

            ruta.Reverse();

            return ruta;
        }

        private static bool Coincide(
            Coordenada primera,
            Coordenada segunda)
        {
            return primera.X == segunda.X &&
                   primera.Y == segunda.Y;
        }
    }
}
