using System;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    /// <summary>
    /// Identifica al Aldeano y la posición del recurso que se quiere recolectar.
    /// </summary>
    public sealed class SolicitudRecoleccion : SolicitudAccion
    {
        public Guid AldeanoId { get; }
        public Coordenada Objetivo { get; }

        public SolicitudRecoleccion(Guid aldeanoId, Coordenada objetivo)
            : base(TipoAccionJuego.Recolectar)
        {
            AldeanoId = aldeanoId;
            Objetivo = objetivo;
        }
    }
}