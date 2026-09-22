using System;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.IA
{
    public enum TipoDecisionMaquina
    {
        Ninguna,
        Recolectar,
        Construir,
        Entrenar
    }

    /// <summary>
    /// Intención de alto nivel producida por la IA. La decisión no modifica
    /// el juego; un controlador de aplicación la ejecuta usando las mismas
    /// operaciones que el jugador humano.
    /// </summary>
    public sealed class DecisionMaquina
    {
        public TipoDecisionMaquina Tipo { get; }
        public Guid UnidadId { get; }
        public Coordenada Objetivo { get; }
        public Coordenada EdificioOrigen { get; }
        public string TipoEdificio { get; }
        public string TipoUnidad { get; }
        public string Motivo { get; }

        private DecisionMaquina(
            TipoDecisionMaquina tipo,
            Guid unidadId,
            Coordenada objetivo,
            Coordenada edificioOrigen,
            string tipoEdificio,
            string tipoUnidad,
            string motivo)
        {
            Tipo = tipo;
            UnidadId = unidadId;
            Objetivo = objetivo;
            EdificioOrigen = edificioOrigen;
            TipoEdificio = tipoEdificio;
            TipoUnidad = tipoUnidad;
            Motivo = motivo ?? string.Empty;
        }

        public static DecisionMaquina SinAccion(
            string motivo)
        {
            return new DecisionMaquina(
                TipoDecisionMaquina.Ninguna,
                Guid.Empty,
                null,
                null,
                null,
                null,
                motivo);
        }

        public static DecisionMaquina Recolectar(
            Guid aldeanoId,
            Coordenada objetivo)
        {
            ValidarUnidad(
                aldeanoId);

            if (objetivo == null)
                throw new ArgumentNullException(
                    nameof(objetivo));

            return new DecisionMaquina(
                TipoDecisionMaquina.Recolectar,
                aldeanoId,
                objetivo,
                null,
                null,
                null,
                "Recolectar el recurso disponible más cercano.");
        }

        public static DecisionMaquina Construir(
            Guid aldeanoId,
            string tipoEdificio,
            Coordenada destino)
        {
            ValidarUnidad(
                aldeanoId);

            if (string.IsNullOrWhiteSpace(
                    tipoEdificio))
            {
                throw new ArgumentException(
                    "El tipo de edificio es obligatorio.",
                    nameof(tipoEdificio));
            }

            if (destino == null)
                throw new ArgumentNullException(
                    nameof(destino));

            return new DecisionMaquina(
                TipoDecisionMaquina.Construir,
                aldeanoId,
                destino,
                null,
                tipoEdificio,
                null,
                "Expandir la base usando un Aldeano disponible.");
        }

        public static DecisionMaquina Entrenar(
            Coordenada edificioOrigen,
            string tipoUnidad)
        {
            if (edificioOrigen == null)
                throw new ArgumentNullException(
                    nameof(edificioOrigen));

            if (string.IsNullOrWhiteSpace(
                    tipoUnidad))
            {
                throw new ArgumentException(
                    "El tipo de unidad es obligatorio.",
                    nameof(tipoUnidad));
            }

            return new DecisionMaquina(
                TipoDecisionMaquina.Entrenar,
                Guid.Empty,
                null,
                edificioOrigen,
                null,
                tipoUnidad,
                "Entrenar una unidad según la prioridad económica actual.");
        }

        private static void ValidarUnidad(
            Guid unidadId)
        {
            if (unidadId == Guid.Empty)
            {
                throw new ArgumentException(
                    "La unidad debe tener un ID válido.",
                    nameof(unidadId));
            }
        }
    }
}
