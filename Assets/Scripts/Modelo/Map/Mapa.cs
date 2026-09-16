using System;
using System.Collections.Generic;
using ImperiosEnGuerra.Modelo.Recursos;

namespace ImperiosEnGuerra.Modelo.Map
{
    public class Mapa
    {
        private readonly Casilla[,] casillas;
        private readonly List<Recurso> recursos;

        public int Ancho { get; }
        public int Alto { get; }

        public IReadOnlyList<Recurso> Recursos
        {
            get { return recursos.AsReadOnly(); }
        }

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
            recursos = new List<Recurso>();

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

        public bool EstaDentroDeLimites(Coordenada coordenada)
        {
            if (coordenada == null)
            {
                return false;
            }

            return coordenada.X >= 0 && coordenada.X < Ancho
                && coordenada.Y >= 0 && coordenada.Y < Alto;
        }

        public bool PuedeColocar(Coordenada coordenada)
        {
            if (!EstaDentroDeLimites(coordenada))
            {
                return false;
            }

            Casilla casilla = ObtenerCasilla(coordenada.X, coordenada.Y);
            return casilla != null && !casilla.EstaOcupada
                && ObtenerRecursoEn(coordenada) == null;
        }

        public Recurso ObtenerRecursoEn(Coordenada coordenada)
        {
            if (!EstaDentroDeLimites(coordenada))
            {
                return null;
            }

            foreach (Recurso recurso in recursos)
            {
                if (recurso.Coordenada.X == coordenada.X
                    && recurso.Coordenada.Y == coordenada.Y)
                {
                    return recurso;
                }
            }

            return null;
        }

        public bool ColocarRecurso(Recurso recurso)
        {
            if (recurso == null)
            {
                return false;
            }

            if (!PuedeColocar(recurso.Coordenada))
            {
                return false;
            }

            recursos.Add(recurso);
            return true;
        }
    }
}