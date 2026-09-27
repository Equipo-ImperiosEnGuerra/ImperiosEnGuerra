using System;
using ImperiosEnGuerra.Modelo.Combate;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Edificios
{
    /// <summary>
    /// Base común de los edificios con identidad, posición y vida de combate.
    /// </summary>
    public abstract class Edificio
    {
        //Protege la vida cuando varios ataques intentan modificarla al mismo tiempo.
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

            //Obtiene la vida inicial desde la configuración central de combate.
            EstadisticasCombate estadisticas =
                new ConfiguracionCombate()
                    .ObtenerParaEdificio(
                        GetType().Name);

            Id = Guid.NewGuid();
            Coordenada = coordenada;
            VidaMaxima = estadisticas.VidaMaxima;
            vidaActual = VidaMaxima;
        }

        //Aplica daño sin permitir que la vida quede por debajo de cero.
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
