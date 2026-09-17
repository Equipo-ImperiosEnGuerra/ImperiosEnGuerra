using System;

namespace ImperiosEnGuerra.Controladores.Red.Contratos
{
    [Serializable]
    public class MoverUnidadDto
    {
        public string unidadId;
        public CoordenadaDto destino;
    }

    [Serializable]
    public class ResultadoAccionDto
    {
        public bool exito;
        public string mensaje;
        public string error;
    }
}
