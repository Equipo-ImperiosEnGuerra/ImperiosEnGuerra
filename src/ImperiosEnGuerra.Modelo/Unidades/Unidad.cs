using System;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Unidades
{
    /// <summary>
    /// Base de las unidades del Modelo, con identidad estable, posición lógica y una marca de disponibilidad.
    /// </summary>
    public abstract class Unidad
    {
        /// <summary>
        /// Identificador estable e inmutable de la unidad durante toda su vida en la partida.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Posición lógica de la unidad; las clases derivadas pueden actualizarla.
        /// </summary>
        public Coordenada Coordenada { get; protected set; }

        /// <summary>
        /// Marca de disponibilidad que puede cambiarse mediante los métodos de la unidad o sus clases derivadas.
        /// </summary>
        public bool Disponible { get; protected set; }

        /// <summary>
        /// Inicializa una unidad con un identificador único, disponible y con la coordenada recibida, sin validarla.
        /// </summary>
        /// <param name="coordenada">Posición lógica inicial.</param>
        protected Unidad(Coordenada coordenada)
        {
            Id = Guid.NewGuid();
            Coordenada = coordenada;
            Disponible = true;
        }

        /// <summary>
        /// Establece la marca de disponibilidad en true.
        /// </summary>
        public void MarcarDisponible()
        {
            Disponible = true;
        }

        /// <summary>
        /// Establece la marca de disponibilidad en false.
        /// </summary>
        public void MarcarNoDisponible()
        {
            Disponible = false;
        }
    }
}