using System;

namespace ImperiosEnGuerra.Controladores.Red.Contratos
{
    [Serializable]
    public class ProcesoIniciadoDto
    {
        public string procesoId;
        public string nombre;
        public string estado;
    }

    [Serializable]
    public class ResultadoProcesoDto
    {
        public string procesoId;
        public string nombre;
        public string estado;
        public int hiloTrabajoId;
        public bool exito;
        public string mensaje;
        public string errorTecnico;
    }
}
