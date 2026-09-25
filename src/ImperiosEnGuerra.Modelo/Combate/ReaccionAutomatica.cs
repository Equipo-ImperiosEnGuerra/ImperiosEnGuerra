using System;

namespace ImperiosEnGuerra.Modelo.Combate
{
    public enum TipoReaccionAutomatica
    {
        Atacar,
        Curar
    }

    public sealed class ReaccionAutomatica
    {
        public TipoReaccionAutomatica Tipo { get; }
        public Guid UnidadId { get; }
        public Guid ObjetivoId { get; }

        public ReaccionAutomatica(
            TipoReaccionAutomatica tipo,
            Guid unidadId,
            Guid objetivoId)
        {
            if (unidadId == Guid.Empty)
                throw new ArgumentException(
                    "La unidad debe tener un ID válido.",
                    nameof(unidadId));

            if (objetivoId == Guid.Empty)
                throw new ArgumentException(
                    "El objetivo debe tener un ID válido.",
                    nameof(objetivoId));

            Tipo = tipo;
            UnidadId = unidadId;
            ObjetivoId = objetivoId;
        }
    }
}
