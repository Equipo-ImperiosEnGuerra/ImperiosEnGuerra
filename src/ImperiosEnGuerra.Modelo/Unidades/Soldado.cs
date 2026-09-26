using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Unidades
{
    /// <summary>
    /// Agrupa las unidades de tipo soldado con la posición y disponibilidad heredadas de Unidad.
    /// </summary>
    public abstract class Soldado : Unidad
    {
        /// <summary>
        /// Inicializa la base del soldado delegando la posición y la disponibilidad inicial en su clase base.
        /// </summary>
        /// <param name="coordenada">Posición lógica inicial, conservada sin validación.</param>
        protected Soldado(
            Coordenada coordenada,
            double velocidadMovimiento = 1d)
            : base(coordenada, velocidadMovimiento)
        {
        }
    }
}