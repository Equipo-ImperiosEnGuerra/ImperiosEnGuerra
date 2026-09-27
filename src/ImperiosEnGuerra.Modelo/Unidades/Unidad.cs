using System;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Combate;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Unidades
{
    /// <summary>
    /// Base común de las unidades: identidad, posición, vida y estado de la orden actual.
    /// </summary>
    public abstract class Unidad
    {
        //Evita que ataques y curaciones cambien la vida al mismo tiempo sin sincronización.
        private readonly object sincronizacionVida =
            new object();

        public Guid Id { get; }
        public Coordenada Coordenada { get; protected set; }
        public bool Disponible { get; protected set; }
        public double VelocidadMovimiento { get; }
        public EstadoUnidad Estado { get; private set; }
        public TipoAccionJuego? OrdenActiva { get; private set; }

        public int VidaMaxima { get; }
        public int DanioAtaque { get; }
        public int AlcanceAtaque { get; }
        public double IntervaloAtaqueSegundos { get; }

        private int vidaActual;

        public int VidaActual
        {
            get
            {
                lock (sincronizacionVida)
                    return vidaActual;
            }
        }

        public bool Destruida =>
            VidaActual <= 0;

        protected Unidad(
            Coordenada coordenada,
            double velocidadMovimiento = 1d)
        {
            if (velocidadMovimiento <= 0d ||
                double.IsNaN(velocidadMovimiento) ||
                double.IsInfinity(velocidadMovimiento))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(velocidadMovimiento),
                    "La velocidad de movimiento debe ser un valor positivo y finito.");
            }

            //Cada tipo de unidad toma sus estadísticas de la configuración central de combate.
            EstadisticasCombate estadisticas =
                new ConfiguracionCombate()
                    .ObtenerParaUnidad(
                        GetType().Name);

            Id = Guid.NewGuid();
            Coordenada = coordenada;
            VelocidadMovimiento = velocidadMovimiento;
            Disponible = true;
            Estado = EstadoUnidad.Idle;
            OrdenActiva = null;

            VidaMaxima = estadisticas.VidaMaxima;
            vidaActual = VidaMaxima;
            DanioAtaque = estadisticas.Danio;
            AlcanceAtaque = estadisticas.Alcance;
            IntervaloAtaqueSegundos =
                estadisticas.IntervaloAtaqueSegundos;
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

        //Recupera vida sin superar el máximo configurado para la unidad.
        public int RecuperarVida(
            int cantidad)
        {
            if (cantidad < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(cantidad));

            lock (sincronizacionVida)
            {
                vidaActual =
                    Math.Min(
                        VidaMaxima,
                        vidaActual + cantidad);

                return vidaActual;
            }
        }

        //Inicia una orden solo si la unidad no tiene otra acción activa.
        public bool IntentarIniciarOrden(TipoAccionJuego tipo)
        {
            if (OrdenActiva.HasValue)
                return false;

            EstadoUnidad? nuevoEstado = EstadoPara(tipo);
            if (!nuevoEstado.HasValue)
                return false;

            OrdenActiva = tipo;
            Estado = nuevoEstado.Value;
            Disponible = false;
            return true;
        }

        //Reemplaza la orden actual cuando el controlador ya autorizó el cambio.
        public bool IntentarReemplazarOrden(TipoAccionJuego tipo)
        {
            EstadoUnidad? nuevoEstado = EstadoPara(tipo);
            if (!nuevoEstado.HasValue)
                return false;

            OrdenActiva = tipo;
            Estado = nuevoEstado.Value;
            Disponible = false;
            return true;
        }

        public void CancelarOrden() =>
            RestablecerOrden();

        public void CompletarOrden() =>
            RestablecerOrden();

        public void MarcarDisponible() =>
            RestablecerOrden();

        public void MarcarNoDisponible()
        {
            Disponible = false;
        }

        //Actualiza la posición lógica después de completar un paso de movimiento.
        internal void EstablecerDestino(Coordenada destino)
        {
            Coordenada = destino;
        }

        //Devuelve la unidad a estado libre cuando una acción termina o se cancela.
        private void RestablecerOrden()
        {
            OrdenActiva = null;
            Estado = EstadoUnidad.Idle;
            Disponible = true;
        }

        //Relaciona cada orden de gameplay con el estado lógico que debe mostrar la unidad.
        private static EstadoUnidad? EstadoPara(TipoAccionJuego tipo)
        {
            switch (tipo)
            {
                case TipoAccionJuego.Mover:
                    return EstadoUnidad.Moviendo;
                case TipoAccionJuego.Recolectar:
                    return EstadoUnidad.Recolectando;
                case TipoAccionJuego.Construir:
                    return EstadoUnidad.Construyendo;
                case TipoAccionJuego.Atacar:
                    return EstadoUnidad.Atacando;
                case TipoAccionJuego.Curar:
                    return EstadoUnidad.Curando;
                default:
                    return null;
            }
        }
    }
}
