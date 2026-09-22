using System;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.IA
{
    public enum TipoDecisionMaquina
    {
        Ninguna,
        Recolectar
    }

    public sealed class DecisionMaquina
    {
        public TipoDecisionMaquina Tipo { get; }
        public Guid UnidadId { get; }
        public Coordenada Objetivo { get; }
        public string Motivo { get; }

        private DecisionMaquina(
            TipoDecisionMaquina tipo,
            Guid unidadId,
            Coordenada objetivo,
            string motivo)
        {
            Tipo = tipo;
            UnidadId = unidadId;
            Objetivo = objetivo;
            Motivo = motivo ?? string.Empty;
        }

        public static DecisionMaquina SinAccion(string motivo)
        {
            return new DecisionMaquina(
                TipoDecisionMaquina.Ninguna,
                Guid.Empty,
                null,
                motivo);
        }

        public static DecisionMaquina Recolectar(
            Guid aldeanoId,
            Coordenada objetivo)
        {
            if (aldeanoId == Guid.Empty)
                throw new ArgumentException(
                    "El Aldeano debe tener un ID válido.",
                    nameof(aldeanoId));

            if (objetivo == null)
                throw new ArgumentNullException(nameof(objetivo));

            return new DecisionMaquina(
                TipoDecisionMaquina.Recolectar,
                aldeanoId,
                objetivo,
                "Recolectar el recurso disponible más cercano.");
        }
    }
}
