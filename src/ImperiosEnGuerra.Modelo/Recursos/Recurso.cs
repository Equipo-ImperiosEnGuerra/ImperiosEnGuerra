using System;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Recursos
{
    /// <summary>
    /// Representa un recurso físico mediante su tipo y posición lógica.
    /// </summary>
    public class Recurso
    {
        /// <summary>
        /// Tipo del recurso físico.
        /// </summary>
        public TipoRecurso Tipo { get; }
        /// <summary>
        /// Posición lógica del recurso físico.
        /// </summary>
        public Coordenada Coordenada { get; }

        /// <summary>
        /// Conserva el tipo y la coordenada recibidos.
        /// </summary>
        /// <param name="tipo">Tipo del recurso.</param>
        /// <param name="coordenada">Posición lógica del recurso.</param>
        /// <exception cref="ArgumentNullException">La coordenada es nula.</exception>
        public Recurso(TipoRecurso tipo, Coordenada coordenada)
        {
            if (coordenada == null)
            {
                throw new ArgumentNullException(nameof(coordenada));
            }

            Tipo = tipo;
            Coordenada = coordenada;
        }
    }
}
