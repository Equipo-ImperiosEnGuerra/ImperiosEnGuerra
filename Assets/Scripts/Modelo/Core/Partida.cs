using System;

namespace ImperiosEnGuerra.Modelo.Core
{
    public class Partida
    {
        public Jugador JugadorHumano { get; }
        public Jugador JugadorMaquina { get; }

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