using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Unidades
{
    /// <summary>
    /// Representa una unidad de combate de tipo Guerrero.
    /// </summary>
    public class Guerrero : Soldado
    {
        /// <summary>
        /// Inicializa el Guerrero en la coordenada indicada.
        /// </summary>
        public Guerrero(Coordenada coordenada)
            : base(coordenada, 1.00d)
        {
        }
    }
}