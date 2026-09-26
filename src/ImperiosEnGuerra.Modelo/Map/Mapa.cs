using System;
using System.Collections.Generic;
using ImperiosEnGuerra.Modelo.Recursos;

namespace ImperiosEnGuerra.Modelo.Map
{
    /// <summary>
    /// Administra una cuadrícula de casillas y los recursos físicos colocados en ella.
    /// </summary>
    public class Mapa
    {
        private readonly Casilla[,] casillas;
        private readonly List<Recurso> recursos;

        /// <summary>
        /// Número de columnas de la cuadrícula.
        /// </summary>
        public int Ancho { get; }
        /// <summary>
        /// Número de filas de la cuadrícula.
        /// </summary>
        public int Alto { get; }

        /// <summary>
        /// Vista de solo lectura de los recursos físicos colocados, actualizada con la colección interna.
        /// </summary>
        public IReadOnlyList<Recurso> Recursos
        {
            get { return recursos.AsReadOnly(); }
        }

        /// <summary>
        /// Crea una cuadrícula de casillas transitables y desocupadas, sin recursos físicos.
        /// </summary>
        /// <param name="ancho">Número de columnas, mayor que cero.</param>
        /// <param name="alto">Número de filas, mayor que cero.</param>
        /// <exception cref="ArgumentOutOfRangeException">El ancho o el alto es menor o igual que cero.</exception>
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

        /// <summary>
        /// Consulta una casilla por sus índices sin modificar el mapa.
        /// </summary>
        /// <param name="x">Índice horizontal desde cero.</param>
        /// <param name="y">Índice vertical desde cero.</param>
        /// <returns>La casilla existente, o null si algún índice queda fuera del mapa.</returns>
        public Casilla ObtenerCasilla(int x, int y)
        {
            if (x < 0 || x >= Ancho || y < 0 || y >= Alto)
            {
                return null;
            }

            return casillas[x, y];
        }

        /// <summary>
        /// Comprueba que ambas componentes estén entre cero incluido y la dimensión correspondiente excluida.
        /// </summary>
        /// <param name="coordenada">Posición que se consulta.</param>
        /// <returns>true si está dentro del mapa; false si es nula o está fuera.</returns>
        public bool EstaDentroDeLimites(Coordenada coordenada)
        {
            if (coordenada == null)
            {
                return false;
            }

            return coordenada.X >= 0 && coordenada.X < Ancho
                && coordenada.Y >= 0 && coordenada.Y < Alto;
        }

        /// <summary>
        /// Consulta si existe una casilla libre y sin recurso físico en la posición, sin evaluar la transitabilidad.
        /// </summary>
        /// <param name="coordenada">Posición que se consulta.</param>
        /// <returns>true si está disponible; false si es nula, está fuera, ocupada o contiene un recurso.</returns>
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

        /// <summary>
        /// Busca un recurso comparando los valores X e Y de su coordenada.
        /// </summary>
        /// <param name="coordenada">Posición lógica que se consulta.</param>
        /// <returns>El recurso encontrado, o null si la posición es nula, está fuera del mapa o no contiene recurso.</returns>
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

        /// <summary>
        /// Añade un recurso si su posición está disponible; no cambia la marca de ocupación de la casilla.
        /// </summary>
        /// <param name="recurso">Recurso físico que se intenta colocar.</param>
        /// <returns>true si se añadió; false si es nulo o su posición no está disponible.</returns>
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


        /// <summary>
        /// Retira un nodo físico agotado. Se usa por el servicio de
        /// regeneración para reemplazarlo por otro nodo del mismo tipo.
        /// </summary>
        public bool RetirarRecursoAgotado(
            Recurso recurso)
        {
            if (recurso == null ||
                !recurso.Agotado)
            {
                return false;
            }

            return recursos.Remove(
                recurso);
        }
    }
}