using System;

namespace ImperiosEnGuerra.Modelo.Core
{
    /// <summary>
    /// Agrupa a los participantes humano y máquina de la partida.
    /// </summary>
    public class Partida
    {
        /// <summary>
        /// Participante de tipo humano.
        /// </summary>
        public Jugador JugadorHumano { get; }
        /// <summary>
        /// Participante de tipo máquina.
        /// </summary>
        public Jugador JugadorMaquina { get; }

        /// <summary>
        /// Asocia los dos participantes después de comprobar sus tipos.
        /// </summary>
        /// <param name="jugadorHumano">Participante que debe ser de tipo Humano.</param>
        /// <param name="jugadorMaquina">Participante que debe ser de tipo Maquina.</param>
        /// <exception cref="ArgumentNullException">Alguno de los participantes es nulo.</exception>
        /// <exception cref="ArgumentException">Alguno de los participantes no tiene el tipo requerido para su posición.</exception>
        public Partida(
            Jugador jugadorHumano,
            Jugador jugadorMaquina)
        {
            if (jugadorHumano == null)
            {
                throw new ArgumentNullException(nameof(jugadorHumano));
            }

            if (jugadorMaquina == null)
            {
                throw new ArgumentNullException(nameof(jugadorMaquina));
            }

            if (jugadorHumano.Tipo != TipoJugador.Humano)
            {
                throw new ArgumentException(
                    "El primer jugador debe ser de tipo Humano.",
                    nameof(jugadorHumano));
            }

            if (jugadorMaquina.Tipo != TipoJugador.Maquina)
            {
                throw new ArgumentException(
                    "El segundo jugador debe ser de tipo Maquina.",
                    nameof(jugadorMaquina));
            }

            JugadorHumano = jugadorHumano;
            JugadorMaquina = jugadorMaquina;
        }
    }
}