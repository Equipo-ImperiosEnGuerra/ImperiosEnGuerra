using System;
using System.Linq;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Core
{
    public class Partida
    {
        public Jugador JugadorHumano { get; }
        public Jugador JugadorMaquina { get; }

        public bool Finalizada { get; private set; }
        public Jugador Ganador { get; private set; }
        public string MotivoFinalizacion { get; private set; }

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
            Finalizada = false;
            Ganador = null;
            MotivoFinalizacion = string.Empty;
        }

        public Jugador ObtenerJugador(
            TipoJugador tipo)
        {
            return tipo == TipoJugador.Humano
                ? JugadorHumano
                : tipo == TipoJugador.Maquina
                    ? JugadorMaquina
                    : null;
        }

        public Jugador BuscarJugadorPorUnidad(
            Guid unidadId)
        {
            if (JugadorHumano.Unidades.Any(u => u.Id == unidadId))
                return JugadorHumano;

            if (JugadorMaquina.Unidades.Any(u => u.Id == unidadId))
                return JugadorMaquina;

            return null;
        }

        public Jugador BuscarJugadorPorEdificio(
            Guid edificioId)
        {
            if (JugadorHumano.Edificios.Any(e => e.Id == edificioId))
                return JugadorHumano;

            if (JugadorMaquina.Edificios.Any(e => e.Id == edificioId))
                return JugadorMaquina;

            return null;
        }

        public Jugador BuscarJugadorPorEdificio(
            Coordenada coordenada)
        {
            if (coordenada == null)
                return null;

            bool humano =
                JugadorHumano.Edificios.Any(
                    e => Coincide(e.Coordenada, coordenada));

            bool maquina =
                JugadorMaquina.Edificios.Any(
                    e => Coincide(e.Coordenada, coordenada));

            if (humano == maquina)
                return null;

            return humano
                ? JugadorHumano
                : JugadorMaquina;
        }

        public Jugador ObtenerOponente(
            Jugador jugador)
        {
            if (ReferenceEquals(jugador, JugadorHumano))
                return JugadorMaquina;

            if (ReferenceEquals(jugador, JugadorMaquina))
                return JugadorHumano;

            return null;
        }

        public bool IntentarFinalizar(
            Jugador ganador,
            string motivo)
        {
            if (ganador == null)
                throw new ArgumentNullException(nameof(ganador));

            if (!ReferenceEquals(ganador, JugadorHumano) &&
                !ReferenceEquals(ganador, JugadorMaquina))
            {
                throw new ArgumentException(
                    "El ganador debe pertenecer a la partida.",
                    nameof(ganador));
            }

            if (Finalizada)
                return false;

            Finalizada = true;
            Ganador = ganador;
            MotivoFinalizacion = motivo ?? string.Empty;
            return true;
        }

        private static bool Coincide(
            Coordenada primera,
            Coordenada segunda)
        {
            return primera != null &&
                   segunda != null &&
                   primera.X == segunda.X &&
                   primera.Y == segunda.Y;
        }
    }
}
