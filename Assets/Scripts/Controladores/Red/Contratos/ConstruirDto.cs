using System;

namespace ImperiosEnGuerra.Controladores.Red.Contratos
{
    [Serializable]
    public class ConstruirDto
    {
        public string aldeanoId;
        public string tipoEdificio;
        public CoordenadaDto destino;
    }
}