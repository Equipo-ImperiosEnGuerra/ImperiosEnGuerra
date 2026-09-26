using System;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    /// <summary>Identifica al Monje curador y a la unidad aliada objetivo mediante IDs estables.</summary>
    public sealed class SolicitudCuracion : SolicitudAccion
    {
        public Guid CuradorId { get; }
        public Guid ObjetivoId { get; }

        public SolicitudCuracion(Guid curadorId, Guid objetivoId)
            : base(TipoAccionJuego.Curar)
        {
            CuradorId = curadorId;
            ObjetivoId = objetivoId;
        }
    }
}
