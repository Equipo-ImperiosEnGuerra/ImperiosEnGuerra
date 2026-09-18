using System;

namespace ImperiosEnGuerra.Controladores.Red.Contratos
{
    [Serializable]
    public class EntrenarDto
    {
        public CoordenadaDto edificioOrigen;
        public string tipoUnidad;
        public CoordenadaDto destino;
    }
}