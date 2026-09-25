using System;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Combate
{
    public enum TipoReaccionAutomatica
    {
        Atacar,
        Curar,
        MoverIdle
    }

    public sealed class ReaccionAutomatica
    {
        public TipoReaccionAutomatica Tipo { get; }
        public Guid UnidadId { get; }
        public Guid ObjetivoId { get; }
        public Coordenada Destino { get; }

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
            Destino = null;
        }

        public ReaccionAutomatica(
            Guid unidadId,
            Coordenada destino)
        {
            if (unidadId == Guid.Empty)
                throw new ArgumentException(
                    "La unidad debe tener un ID válido.",
                    nameof(unidadId));

            if (destino == null)
                throw new ArgumentNullException(
                    nameof(destino));

            Tipo =
                TipoReaccionAutomatica.MoverIdle;

            UnidadId = unidadId;
            ObjetivoId = Guid.Empty;
            Destino = destino;
        }
    }
}
