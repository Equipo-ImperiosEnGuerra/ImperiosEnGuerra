using System;

namespace ImperiosEnGuerra.Controladores.Red.Contratos
{
    /// <summary>
    /// Datos que Unity envía para entrenar una unidad desde un edificio.
    /// </summary>
    [Serializable]
    public class EntrenarDto
    {
        public CoordenadaDto edificioOrigen;
        public string tipoUnidad;
        public CoordenadaDto destino;
    }
}