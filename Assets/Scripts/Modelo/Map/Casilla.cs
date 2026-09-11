namespace ImperiosEnGuerra.Modelo.Map
{
    public class Casilla
    {
        public Coordenada Posicion { get; }
        public bool EsTransitable { get; private set; }

        public Casilla(Coordenada posicion, bool esTransitable)
        {
            Posicion = posicion;
            EsTransitable = esTransitable;
        }

        public void CambiarTransitabilidad(bool esTransitable)
        {
            EsTransitable = esTransitable;
        }
    }
}