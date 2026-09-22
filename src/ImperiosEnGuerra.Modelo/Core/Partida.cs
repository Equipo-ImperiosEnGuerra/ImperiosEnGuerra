using System;
using System.Linq;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Core
{
    /// <summary>
    /// Agrupa a los participantes humano y máquina de la partida.
    /// </summary>
    public class Partida
    {
        public Jugador JugadorHumano { get; }
        public Jugador JugadorMaquina { get; }

        public Partida(
            Jugador jugadorHumano,
            Jugador jugadorMaquina)
        {
            if (jugadorHumano == null)
                throw new ArgumentNullException(nameof(jugadorHumano));

            if (jugadorMaquina == null)
                throw new ArgumentNullException(nameof(jugadorMaquina));

            if (jugadorHumano.Tipo != TipoJugador.Humano)
                throw new ArgumentException(
                    "El primer jugador debe ser de tipo Humano.",
                    nameof(jugadorHumano));

            if (jugadorMaquina.Tipo != TipoJugador.Maquina)
                throw new ArgumentException(
                    "El segundo jugador debe ser de tipo Maquina.",
                    nameof(jugadorMaquina));

            JugadorHumano = jugadorHumano;
            JugadorMaquina = jugadorMaquina;
        }

        /// <summary>
        /// Localiza al propietario de una unidad sin asumir si es Humano o Máquina.
        /// </summary>
        public Jugador BuscarJugadorPorUnidad(Guid unidadId)
        {
            if (JugadorHumano.Unidades.Any(
                    u => u.Id == unidadId))
            {
                return JugadorHumano;
            }

            if (JugadorMaquina.Unidades.Any(
                    u => u.Id == unidadId))
            {
                return JugadorMaquina;
            }

            return null;
        }

        /// <summary>
        /// Devuelve el oponente del jugador recibido.
        /// </summary>
        public Jugador ObtenerOponente(Jugador jugador)
        {
            if (ReferenceEquals(
                    jugador,
                    JugadorHumano))
            {
                return JugadorMaquina;
            }

            if (ReferenceEquals(
                    jugador,
                    JugadorMaquina))
            {
                return JugadorHumano;
            }

            return null;
        }
    }
}
