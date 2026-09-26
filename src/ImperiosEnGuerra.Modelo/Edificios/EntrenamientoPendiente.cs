using System;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Edificios
{
    public sealed class EntrenamientoPendiente
    {
        public Guid Id { get; }
        public string TipoUnidad { get; }
        public Coordenada PuntoReunion { get; }
        public int Progreso { get; private set; }

        public EntrenamientoPendiente(
            string tipoUnidad,
            Coordenada puntoReunion)
        {
            if (string.IsNullOrWhiteSpace(tipoUnidad))
            {
                throw new ArgumentException(
                    "El tipo de unidad es obligatorio.",
                    nameof(tipoUnidad));
            }

            Id = Guid.NewGuid();
            TipoUnidad = tipoUnidad;
            PuntoReunion = puntoReunion;
            Progreso = 0;
        }

        public int Avanzar(
            int incremento)
        {
            if (incremento <= 0)
                throw new ArgumentOutOfRangeException(nameof(incremento));

            Progreso =
                Math.Min(
                    100,
                    Progreso + incremento);

            return Progreso;
        }
    }
}
