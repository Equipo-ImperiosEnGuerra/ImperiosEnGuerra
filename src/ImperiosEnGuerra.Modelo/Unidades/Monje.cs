using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Unidades
{
    /// <summary>
    /// Representa una unidad de tipo Monje.
    /// </summary>
    public class Monje : Unidad
    {
        /// <summary>
        /// Inicializa el Monje en la coordenada indicada.
        /// </summary>
        public Monje(Coordenada coordenada)
            : base(coordenada, 0.80d)
        {
        }
    }
}