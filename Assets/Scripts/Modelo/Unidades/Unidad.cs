using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Unidades
{
    public abstract class Unidad
    {
        public Coordenada Coordenada { get; protected set; }
        public bool Disponible { get; protected set; }

        protected Unidad(Coordenada coordenada)
        {
            Coordenada = coordenada;
            Disponible = true;
        }

        public void MarcarDisponible()
        {
            Disponible = true;
        }

        public void MarcarNoDisponible()
        {
            Disponible = false;
        }
    }
}