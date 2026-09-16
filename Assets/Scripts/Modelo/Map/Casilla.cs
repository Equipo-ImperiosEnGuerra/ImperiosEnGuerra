namespace ImperiosEnGuerra.Modelo.Map
{
    public class Casilla
    {
        public Coordenada Posicion { get; }
        public bool EsTransitable { get; private set; }
        public bool EstaOcupada { get; private set; }

        public Casilla(Coordenada posicion, bool esTransitable)
        {
            Posicion = posicion;
            EsTransitable = esTransitable;
            EstaOcupada = false;
        }

        public void CambiarTransitabilidad(bool esTransitable)
        {
            EsTransitable = esTransitable;
        }

        public bool Ocupar()
        {
            if (EstaOcupada)
            {
                return false;
            }

            EstaOcupada = true;
            return true;
        }

        public void Liberar()
        {
            EstaOcupada = false;
        }
    }
}