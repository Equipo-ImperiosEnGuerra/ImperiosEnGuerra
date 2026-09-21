using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Unidades
{
    /// <summary>
    /// Representa la especialización Aldeano con la posición y disponibilidad heredadas.
    /// </summary>
    public class Aldeano : Unidad
    {
        /// <summary>
        /// Inicializa la unidad delegando la posición y la disponibilidad inicial en su clase base.
        /// </summary>
        /// <param name="coordenada">Posición lógica inicial, conservada sin validación.</param>
        public Aldeano(Coordenada coordenada)
            : base(coordenada, 1.00d)
        {
        }
    }
}