using System;
using ImperiosEnGuerra.Modelo.Combate;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Edificios
{
    public abstract class Edificio
    {
        private readonly object sincronizacionVida =
            new object();

        private int vidaActual;

        public Guid Id { get; }
        public Coordenada Coordenada { get; }
        public int VidaMaxima { get; }

        public int VidaActual
        {
            get
            {
                lock (sincronizacionVida)
                    return vidaActual;
            }
        }

        public bool Destruido =>
            VidaActual <= 0;

        protected Edificio(Coordenada coordenada)
        {
            if (coordenada == null)
                throw new ArgumentNullException(nameof(coordenada));

            EstadisticasCombate estadisticas =
                new ConfiguracionCombate()
                    .ObtenerParaEdificio(
                        GetType().Name);

            Id = Guid.NewGuid();
            Coordenada = coordenada;
            VidaMaxima = estadisticas.VidaMaxima;
            vidaActual = VidaMaxima;
        }

        public int RecibirDanio(
            int cantidad)
        {
            if (cantidad < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(cantidad));

            lock (sincronizacionVida)
            {
                vidaActual =
                    Math.Max(
                        0,
                        vidaActual - cantidad);

                return vidaActual;
            }
        }
    }
}
