using System;

namespace ImperiosEnGuerra.Controladores.Red.Contratos
{
    /// <summary>
    /// Identifica la unidad y la casilla destino enviadas a la API.
    /// </summary>
    [Serializable]
    public class MoverUnidadDto
    {
        public string unidadId;
        public CoordenadaDto destino;
    }

    /// <summary>
    /// Respuesta simple de una acción síncrona recibida desde la API.
    /// </summary>
    [Serializable]
    public class ResultadoAccionDto
    {
        public bool exito;
        public string mensaje;
        public string error;
    }
}
