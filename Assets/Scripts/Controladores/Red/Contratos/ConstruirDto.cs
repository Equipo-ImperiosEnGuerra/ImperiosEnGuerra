using System;

namespace ImperiosEnGuerra.Controladores.Red.Contratos
{
    /// <summary>
    /// Datos que Unity envía para construir un edificio en una casilla.
    /// </summary>
    [Serializable]
    public class ConstruirDto
    {
        public string aldeanoId;
        public string tipoEdificio;
        public CoordenadaDto destino;
    }
}