using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Unidades
{
    /// <summary>
    /// Representa una unidad de combate de tipo Lancero.
    /// </summary>
    public class Lancero : Soldado
    {
        /// <summary>
        /// Inicializa el Lancero en la coordenada indicada.
        /// </summary>
        public Lancero(Coordenada coordenada)
            : base(coordenada, 1.20d)
        {
        }
    }
}