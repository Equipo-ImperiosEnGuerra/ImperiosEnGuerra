using System;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    /// <summary>Identifica la unidad y el destino lógico; la operación valida la solicitud.</summary>
    public sealed class SolicitudMovimiento : SolicitudAccion
    {
        public Guid UnidadId { get; }
        public Coordenada Destino { get; }

        public SolicitudMovimiento(Guid unidadId, Coordenada destino)
            : base(TipoAccionJuego.Mover)
        {
            UnidadId = unidadId;
            Destino = destino;
        }
    }
}
