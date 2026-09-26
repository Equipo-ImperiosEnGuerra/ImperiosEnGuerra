using System;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.IA
{
    public enum TipoDecisionMaquina
    {
        Ninguna,
        Recolectar,
        Construir,
        Entrenar,
        Mover,
        Patrullar,
        Atacar
    }

    public sealed class DecisionMaquina
    {
        public TipoDecisionMaquina Tipo { get; }
        public Guid UnidadId { get; }
        public Guid ObjetivoUnidadId { get; }
        public Coordenada Objetivo { get; }
        public Coordenada EdificioOrigen { get; }
        public string TipoEdificio { get; }
        public string TipoUnidad { get; }
        public string Motivo { get; }

        private DecisionMaquina(
            TipoDecisionMaquina tipo,
            Guid unidadId,
            Guid objetivoUnidadId,
            Coordenada objetivo,
            Coordenada edificioOrigen,
            string tipoEdificio,
            string tipoUnidad,
            string motivo)
        {
            Tipo = tipo;
            UnidadId = unidadId;
            ObjetivoUnidadId = objetivoUnidadId;
            Objetivo = objetivo;
            EdificioOrigen = edificioOrigen;
            TipoEdificio = tipoEdificio;
            TipoUnidad = tipoUnidad;
            Motivo = motivo ?? string.Empty;
        }

        public static DecisionMaquina SinAccion(string motivo)
        {
            return new DecisionMaquina(
                TipoDecisionMaquina.Ninguna,
                Guid.Empty,
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
            ValidarUnidad(aldeanoId);

            if (objetivo == null)
                throw new ArgumentNullException(nameof(objetivo));

            return new DecisionMaquina(
                TipoDecisionMaquina.Recolectar,
                aldeanoId,
                Guid.Empty,
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
            ValidarUnidad(aldeanoId);

            if (string.IsNullOrWhiteSpace(tipoEdificio))
                throw new ArgumentException(
                    "El tipo de edificio es obligatorio.",
                    nameof(tipoEdificio));

            if (destino == null)
                throw new ArgumentNullException(nameof(destino));

            return new DecisionMaquina(
                TipoDecisionMaquina.Construir,
                aldeanoId,
                Guid.Empty,
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
                throw new ArgumentNullException(nameof(edificioOrigen));

            if (string.IsNullOrWhiteSpace(tipoUnidad))
                throw new ArgumentException(
                    "El tipo de unidad es obligatorio.",
                    nameof(tipoUnidad));

            return new DecisionMaquina(
                TipoDecisionMaquina.Entrenar,
                Guid.Empty,
                Guid.Empty,
                null,
                edificioOrigen,
                null,
                tipoUnidad,
                "Entrenar una unidad según la prioridad económica actual.");
        }

        public static DecisionMaquina Mover(
            Guid unidadId,
            Coordenada destino,
            Guid objetivoUnidadId)
        {
            ValidarUnidad(unidadId);

            if (destino == null)
                throw new ArgumentNullException(nameof(destino));

            return new DecisionMaquina(
                TipoDecisionMaquina.Mover,
                unidadId,
                objetivoUnidadId,
                destino,
                null,
                null,
                null,
                "Acercar una unidad militar al enemigo.");
        }

        public static DecisionMaquina Patrullar(
            Guid unidadId,
            Coordenada destino)
        {
            ValidarUnidad(unidadId);

            if (destino == null)
                throw new ArgumentNullException(nameof(destino));

            return new DecisionMaquina(
                TipoDecisionMaquina.Patrullar,
                unidadId,
                Guid.Empty,
                destino,
                null,
                null,
                null,
                "Patrullar una zona cercana mientras no haya una prioridad mayor.");
        }

        public static DecisionMaquina Atacar(
            Guid unidadId,
            Guid objetivoUnidadId)
        {
            ValidarUnidad(unidadId);

            if (objetivoUnidadId == Guid.Empty)
                throw new ArgumentException(
                    "El objetivo debe tener un ID válido.",
                    nameof(objetivoUnidadId));

            return new DecisionMaquina(
                TipoDecisionMaquina.Atacar,
                unidadId,
                objetivoUnidadId,
                null,
                null,
                null,
                null,
                "Preparar ataque contra la unidad enemiga más cercana.");
        }

        private static void ValidarUnidad(Guid unidadId)
        {
            if (unidadId == Guid.Empty)
                throw new ArgumentException(
                    "La unidad debe tener un ID válido.",
                    nameof(unidadId));
        }
    }
}
