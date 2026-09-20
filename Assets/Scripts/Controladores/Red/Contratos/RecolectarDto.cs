using System;

namespace ImperiosEnGuerra.Controladores.Red.Contratos
{
    [Serializable]
    public class RecolectarDto
    {
        public string aldeanoId;
        public CoordenadaDto objetivo;
    }
}