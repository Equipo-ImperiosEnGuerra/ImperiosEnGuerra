using System;
using System.Collections.Generic;
using System.Linq;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Modelo.Core
{
    public class Partida
    {
        private readonly List<Jugador> jugadores;

        public Jugador JugadorHumano { get; }

        /// <summary>
        /// Compatibilidad con la versión Humano vs Máquina.
        /// Devuelve la primera IA de la colección.
        /// </summary>
        public Jugador JugadorMaquina =>
            JugadoresMaquina.FirstOrDefault();

        public IReadOnlyList<Jugador> Jugadores =>
            jugadores.AsReadOnly();

        public IReadOnlyList<Jugador> JugadoresMaquina =>
            jugadores
                .Where(
                    j => j.Tipo == TipoJugador.Maquina)
                .ToList()
                .AsReadOnly();

        public bool Finalizada { get; private set; }
        public Jugador Ganador { get; private set; }
        public string MotivoFinalizacion { get; private set; }

        public Partida(
            Jugador jugadorHumano,
            Jugador jugadorMaquina)
            : this(
                jugadorHumano,
                CrearListaMaquina(
                    jugadorMaquina),
                true)
        {
        }

        private static IEnumerable<Jugador> CrearListaMaquina(
            Jugador jugadorMaquina)
        {
            if (jugadorMaquina == null)
            {
                throw new ArgumentNullException(
                    nameof(jugadorMaquina));
            }

            return new[]
            {
                jugadorMaquina
            };
        }

        public static Partida CrearMultijugador(
            Jugador jugadorHumano,
            IEnumerable<Jugador> jugadoresMaquina)
        {
            return new Partida(
                jugadorHumano,
                jugadoresMaquina,
                true);
        }

        private Partida(
            Jugador jugadorHumano,
            IEnumerable<Jugador> jugadoresMaquina,
            bool multijugador)
        {
            if (jugadorHumano == null)
                throw new ArgumentNullException(
                    nameof(jugadorHumano));

            if (jugadoresMaquina == null)
                throw new ArgumentNullException(
                    nameof(jugadoresMaquina));

            if (jugadorHumano.Tipo != TipoJugador.Humano)
            {
                throw new ArgumentException(
                    "El primer jugador debe ser de tipo Humano.",
                    nameof(jugadorHumano));
            }

            List<Jugador> maquinas =
                jugadoresMaquina
                    .Where(j => j != null)
                    .ToList();

            if (maquinas.Count == 0)
            {
                throw new ArgumentException(
                    "La partida requiere al menos un jugador Máquina.",
                    nameof(jugadoresMaquina));
            }

            if (maquinas.Any(
                    j => j.Tipo != TipoJugador.Maquina))
            {
                throw new ArgumentException(
                    "Todos los rivales deben ser de tipo Maquina.",
                    nameof(jugadoresMaquina));
            }

            if (maquinas.Distinct().Count() !=
                maquinas.Count)
            {
                throw new ArgumentException(
                    "No se puede registrar dos veces el mismo jugador.",
                    nameof(jugadoresMaquina));
            }

            JugadorHumano = jugadorHumano;

            jugadores =
                new List<Jugador>
                {
                    jugadorHumano
                };

            jugadores.AddRange(
                maquinas);

            Finalizada = false;
            Ganador = null;
            MotivoFinalizacion = string.Empty;
        }

        public Jugador ObtenerJugador(
            TipoJugador tipo)
        {
            return jugadores.FirstOrDefault(
                j => j.Tipo == tipo);
        }

        public IReadOnlyList<Jugador> ObtenerJugadores(
            TipoJugador tipo)
        {
            return jugadores
                .Where(j => j.Tipo == tipo)
                .ToList()
                .AsReadOnly();
        }

        public Jugador BuscarJugadorPorUnidad(
            Guid unidadId)
        {
            return jugadores.FirstOrDefault(
                jugador =>
                    jugador.Unidades.Any(
                        u => u.Id == unidadId));
        }

        public Jugador BuscarJugadorPorEdificio(
            Guid edificioId)
        {
            return jugadores.FirstOrDefault(
                jugador =>
                    jugador.Edificios.Any(
                        e => e.Id == edificioId));
        }

        public Jugador BuscarJugadorPorEdificio(
            Coordenada coordenada)
        {
            if (coordenada == null)
                return null;

            Jugador[] coincidencias =
                jugadores
                    .Where(
                        jugador =>
                            jugador.Edificios.Any(
                                e => Coincide(
                                    e.Coordenada,
                                    coordenada)))
                    .ToArray();

            return coincidencias.Length == 1
                ? coincidencias[0]
                : null;
        }

        public IReadOnlyList<Jugador> ObtenerEnemigos(
            Jugador jugador)
        {
            if (jugador == null ||
                !jugadores.Contains(jugador))
            {
                return Array.Empty<Jugador>();
            }

            if (ReferenceEquals(
                    jugador,
                    JugadorHumano))
            {
                return JugadoresMaquina;
            }

            // Las IAs pertenecen al mismo equipo durante esta fase.
            return new[] { JugadorHumano };
        }

        public Jugador ObtenerOponente(
            Jugador jugador)
        {
            return ObtenerEnemigos(
                    jugador)
                .FirstOrDefault();
        }

        public bool SonAliados(
            Jugador primero,
            Jugador segundo)
        {
            if (primero == null ||
                segundo == null)
            {
                return false;
            }

            if (ReferenceEquals(
                    primero,
                    segundo))
            {
                return true;
            }

            return primero.Tipo == TipoJugador.Maquina &&
                   segundo.Tipo == TipoJugador.Maquina;
        }

        public bool SonEnemigos(
            Jugador primero,
            Jugador segundo)
        {
            if (primero == null ||
                segundo == null)
            {
                return false;
            }

            return jugadores.Contains(primero) &&
                   jugadores.Contains(segundo) &&
                   !SonAliados(
                       primero,
                       segundo);
        }

        public bool IntentarFinalizar(
            Jugador ganador,
            string motivo)
        {
            if (ganador == null)
                throw new ArgumentNullException(
                    nameof(ganador));

            if (!jugadores.Contains(
                    ganador))
            {
                throw new ArgumentException(
                    "El ganador debe pertenecer a la partida.",
                    nameof(ganador));
            }

            if (Finalizada)
                return false;

            Finalizada = true;
            Ganador = ganador;
            MotivoFinalizacion =
                motivo ?? string.Empty;

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
