using System;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Unidades
{
    /// <summary>
    /// Base de las unidades del Modelo, con identidad estable, posición lógica y estado de gameplay.
    /// </summary>
    public abstract class Unidad
    {
        /// <summary>
        /// Identificador estable e inmutable de la unidad durante toda su vida en la partida.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Posición lógica de la unidad; las clases derivadas pueden actualizarla.
        /// </summary>
        public Coordenada Coordenada { get; protected set; }

        /// <summary>
        /// Marca de disponibilidad conservada por compatibilidad con las validaciones existentes.
        /// Una orden activa vuelve la unidad no disponible hasta completarse o cancelarse.
        /// </summary>
        public bool Disponible { get; protected set; }

        /// <summary>
        /// Velocidad lógica expresada como multiplicador de casillas por unidad de tiempo.
        /// Un valor mayor hace que el intervalo entre pasos sea menor.
        /// </summary>
        public double VelocidadMovimiento { get; }

        /// <summary>
        /// Estado lógico autoritativo de la unidad.
        /// </summary>
        public EstadoUnidad Estado { get; private set; }

        /// <summary>
        /// Tipo de orden activa. Es null cuando la unidad está Idle.
        /// </summary>
        public TipoAccionJuego? OrdenActiva { get; private set; }

        /// <summary>
        /// Inicializa una unidad con un identificador único, disponible, sin orden y en estado Idle.
        /// </summary>
        /// <param name="coordenada">Posición lógica inicial.</param>
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

            Id = Guid.NewGuid();
            Coordenada = coordenada;
            VelocidadMovimiento = velocidadMovimiento;
            Disponible = true;
            Estado = EstadoUnidad.Idle;
            OrdenActiva = null;
        }

        /// <summary>
        /// Intenta iniciar una orden de unidad. Solo puede existir una orden activa a la vez.
        /// </summary>
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

        /// <summary>
        /// Reemplaza explícitamente una orden activa por otra compatible.
        /// Si la nueva orden no corresponde a un estado de unidad, no modifica el estado actual.
        /// </summary>
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

        /// <summary>
        /// Cancela la orden actual y devuelve la unidad a Idle.
        /// </summary>
        public void CancelarOrden()
        {
            RestablecerOrden();
        }

        /// <summary>
        /// Marca la orden como completada y devuelve la unidad a Idle.
        /// </summary>
        public void CompletarOrden()
        {
            RestablecerOrden();
        }

        /// <summary>
        /// Establece la marca de disponibilidad en true y limpia cualquier orden pendiente.
        /// </summary>
        public void MarcarDisponible()
        {
            RestablecerOrden();
        }

        /// <summary>
        /// Establece la marca de disponibilidad en false.
        /// </summary>
        public void MarcarNoDisponible()
        {
            Disponible = false;
        }

        /// <summary>Actualiza la posición desde la operación de movimiento del Modelo, después de validarla.</summary>
        internal void EstablecerDestino(Coordenada destino)
        {
            Coordenada = destino;
        }

        private void RestablecerOrden()
        {
            OrdenActiva = null;
            Estado = EstadoUnidad.Idle;
            Disponible = true;
        }

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
                default:
                    return null;
            }
        }
    }
}
