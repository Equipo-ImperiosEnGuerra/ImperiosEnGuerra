using System;

namespace ImperiosEnGuerra.Controladores.Red.Contratos
{
    /// <summary>
    /// Identifica al Aldeano y el recurso que Unity pide recolectar.
    /// </summary>
    [Serializable]
    public class RecolectarDto
    {
        public string aldeanoId;
        public CoordenadaDto objetivo;
    }
}