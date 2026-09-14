using System;

namespace ImperiosEnGuerra.Modelo.Map
{
    public class Mapa
    {
        private readonly Casilla[,] casillas;

        public int Ancho { get; }
        public int Alto { get; }

        public Mapa(int ancho, int alto)
        {
            if (ancho <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ancho));
            }

            if (alto <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(alto));
            }

            Ancho = ancho;
            Alto = alto;

            casillas = new Casilla[ancho, alto];

            for (int x = 0; x < ancho; x++)
            {
                for (int y = 0; y < alto; y++)
                {
                    casillas[x, y] = new Casilla(
                        new Coordenada(x, y),
                        true
                    );
                }
            }
        }

        public Casilla ObtenerCasilla(int x, int y)
        {
            if (x < 0 || x >= Ancho || y < 0 || y >= Alto)
            {
                return null;
            }

            return casillas[x, y];
        }
    }
}