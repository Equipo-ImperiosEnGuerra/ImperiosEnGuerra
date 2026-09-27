using System;

namespace ImperiosEnGuerra.Controladores.Red.Contratos
{
    /// <summary>
    /// Datos que Unity envía a la API para iniciar un ataque.
    /// </summary>
    [Serializable]
    public class AtaqueDto
    {
        public string atacanteId;
        public string objetivoId;
    }
}
