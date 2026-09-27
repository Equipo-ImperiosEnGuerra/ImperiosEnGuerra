using System;

namespace ImperiosEnGuerra.Controladores.Red.Contratos
{
    /// <summary>
    /// Datos que Unity envía para ordenar una curación.
    /// </summary>
    [Serializable]
    public class CuracionDto
    {
        public string curadorId;
        public string objetivoId;
    }
}
