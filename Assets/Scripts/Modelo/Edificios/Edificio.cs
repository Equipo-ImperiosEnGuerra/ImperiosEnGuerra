using System;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Edificios
{
    /// <summary>
    /// Base de las construcciones del Modelo, definida por una posición lógica.
    /// </summary>
    public abstract class Edificio
    {
        /// <summary>
        /// Posición lógica de la construcción, establecida al crearla.
        /// </summary>
        public Coordenada Coordenada { get; }

        /// <summary>
        /// Inicializa la posición común de las construcciones.
        /// </summary>
        /// <param name="coordenada">Posición lógica no nula.</param>
        /// <exception cref="ArgumentNullException">La coordenada es nula.</exception>
        protected Edificio(Coordenada coordenada)
        {
            if (coordenada == null)
            {
                throw new ArgumentNullException(nameof(coordenada));
            }

            Coordenada = coordenada;
        }
    }
}
