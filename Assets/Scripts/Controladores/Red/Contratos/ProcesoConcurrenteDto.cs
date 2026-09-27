using System;

namespace ImperiosEnGuerra.Controladores.Red.Contratos
{
    /// <summary>
    /// Identificador devuelto cuando la API acepta una acción concurrente.
    /// </summary>
    [Serializable]
    public class ProcesoIniciadoDto
    {
        public string procesoId;
        public string nombre;
        public string estado;
    }

    /// <summary>
    /// Resultado publicado por el worker que ejecutó la acción en segundo plano.
    /// </summary>
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
