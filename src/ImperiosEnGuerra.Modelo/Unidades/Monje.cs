using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Unidades
{
    /// <summary>
    /// Representa una unidad de tipo Monje.
    /// </summary>
    public class Monje : Unidad
    {
        public int CantidadCuracion { get; } = 15;
        public int AlcanceCuracion { get; } = 2;
        public double IntervaloCuracionSegundos { get; } = 5d;

        /// <summary>
        /// Inicializa el Monje en la coordenada indicada.
        /// </summary>
        public Monje(Coordenada coordenada)
            : base(coordenada, 1.20d)
        {
        }
    }
}