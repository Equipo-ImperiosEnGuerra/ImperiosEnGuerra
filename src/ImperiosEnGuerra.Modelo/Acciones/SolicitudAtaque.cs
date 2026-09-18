using System;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    /// <summary>Identifica atacante y objetivo mediante IDs estables; la operación valida la intención.</summary>
    public sealed class SolicitudAtaque : SolicitudAccion
    {
        public Guid AtacanteId { get; }
        public Guid ObjetivoId { get; }

        public SolicitudAtaque(Guid atacanteId, Guid objetivoId)
            : base(TipoAccionJuego.Atacar)
        {
            AtacanteId = atacanteId;
            ObjetivoId = objetivoId;
        }
    }
}
