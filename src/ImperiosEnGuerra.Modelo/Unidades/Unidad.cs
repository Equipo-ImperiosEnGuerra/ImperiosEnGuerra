using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Unidades
{
    /// <summary>
    /// Base de las unidades del Modelo, con posición lógica y una marca de disponibilidad.
    /// </summary>
    public abstract class Unidad
    {
        /// <summary>
        /// Posición lógica de la unidad; las clases derivadas pueden actualizarla.
        /// </summary>
        public Coordenada Coordenada { get; protected set; }
        /// <summary>
        /// Marca de disponibilidad que puede cambiarse mediante los métodos de la unidad o sus clases derivadas.
        /// </summary>
        public bool Disponible { get; protected set; }

        /// <summary>
        /// Inicializa una unidad disponible con la coordenada recibida, sin validarla.
        /// </summary>
        /// <param name="coordenada">Posición lógica inicial.</param>
        protected Unidad(Coordenada coordenada)
        {
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