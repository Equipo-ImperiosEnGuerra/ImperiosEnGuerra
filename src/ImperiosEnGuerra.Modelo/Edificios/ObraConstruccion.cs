using System;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Edificios
{
    /// <summary>
    /// Estado lógico de una construcción todavía no terminada.
    /// </summary>
    public sealed class ObraConstruccion
    {
        public Guid Id { get; }
        public Guid AldeanoId { get; }
        public string TipoEdificio { get; }
        public Coordenada Coordenada { get; }
        public int Progreso { get; private set; }

        public bool Terminada =>
            Progreso >= 100;

        public ObraConstruccion(
            Guid aldeanoId,
            string tipoEdificio,
            Coordenada coordenada)
        {
            if (string.IsNullOrWhiteSpace(tipoEdificio))
            {
                throw new ArgumentException(
                    "El tipo de edificio es obligatorio.",
                    nameof(tipoEdificio));
            }

            if (coordenada == null)
            {
                throw new ArgumentNullException(
                    nameof(coordenada));
            }

            Id = Guid.NewGuid();
            AldeanoId = aldeanoId;
            TipoEdificio = tipoEdificio;
            Coordenada = coordenada;
            Progreso = 0;
        }

        public int Avanzar(
            int incremento)
        {
            if (incremento <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(incremento));
            }

            Progreso =
                Math.Min(
                    100,
                    Progreso + incremento);

            return Progreso;
        }
    }
}
