using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Unidades
{
    /// <summary>
    /// Representa la especialización Caballero con la posición y disponibilidad heredadas.
    /// </summary>
    public class Caballero : Soldado
    {
        /// <summary>
        /// Inicializa la unidad delegando la posición y la disponibilidad inicial en su clase base.
        /// </summary>
        /// <param name="coordenada">Posición lógica inicial, conservada sin validación.</param>
        public Caballero(Coordenada coordenada)
        : base (coordenada)
        {
            
        }
    }
}
