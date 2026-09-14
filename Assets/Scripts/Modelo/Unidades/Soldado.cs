using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Unidades
{
    public abstract class Soldado : Unidad
    {
        protected Soldado(Coordenada coordenada)
            : base(coordenada)
        {
        }
    }
}