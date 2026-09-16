using System;
using System.Collections.Generic;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;

namespace ImperiosEnGuerra.Modelo.Core
{
    /// <summary>
    /// Valida y configura los mapas y participantes que forman el estado inicial de una partida.
    /// </summary>
    public class InicializadorPartida
    {
        /// <summary>
        /// Valida ambos conjuntos de posiciones antes de configurar los mapas y crea cada jugador con saldos cero y un Centro Urbano.
        /// La validación evita solapamientos también cuando los participantes comparten un mapa.
        /// </summary>
        /// <param name="nombreHumano">Nombre del participante humano.</param>
        /// <param name="mapaHumano">Mapa asociado al humano.</param>
        /// <param name="centroHumano">Posición del Centro Urbano humano.</param>
        /// <param name="recursosHumano">Recursos físicos del humano; deben incluir oro, madera y comida.</param>
        /// <param name="nombreMaquina">Nombre del participante máquina.</param>
        /// <param name="mapaMaquina">Mapa asociado a la máquina.</param>
        /// <param name="centroMaquina">Posición del Centro Urbano de la máquina.</param>
        /// <param name="recursosMaquina">Recursos físicos de la máquina; deben incluir oro, madera y comida.</param>
        /// <returns>Partida con ambos participantes y los mapas recibidos configurados.</returns>
        /// <exception cref="ArgumentNullException">Algún mapa, centro o lista de recursos es nulo.</exception>
        /// <exception cref="ArgumentException">Algún nombre es inválido, hay recursos nulos, posiciones no disponibles o repetidas, o falta un tipo requerido.</exception>
        /// <exception cref="InvalidOperationException">No se puede ocupar un centro o colocar un recurso al aplicar la configuración validada.</exception>
        public Partida Crear(
            string nombreHumano,
            Mapa mapaHumano,
            Coordenada centroHumano,
            IReadOnlyList<Recurso> recursosHumano,
            string nombreMaquina,
            Mapa mapaMaquina,
            Coordenada centroMaquina,
            IReadOnlyList<Recurso> recursosMaquina)
        {
            var posicionesPorMapa = new Dictionary<Mapa, HashSet<(int, int)>>();

            ValidarMapa(mapaHumano, centroHumano, recursosHumano, posicionesPorMapa);
            ValidarMapa(mapaMaquina, centroMaquina, recursosMaquina, posicionesPorMapa);

            Jugador jugadorHumano = new Jugador(
                nombreHumano, TipoJugador.Humano, mapaHumano, new RecursosJugador());
            Jugador jugadorMaquina = new Jugador(
                nombreMaquina, TipoJugador.Maquina, mapaMaquina, new RecursosJugador());

            ConfigurarMapa(jugadorHumano, centroHumano, recursosHumano);
            ConfigurarMapa(jugadorMaquina, centroMaquina, recursosMaquina);

            return new Partida(jugadorHumano, jugadorMaquina);
        }

        /// <summary>
        /// Valida posiciones y tipos requeridos sin modificar el mapa; reserva las posiciones en el registro compartido.
        /// </summary>
        /// <param name="mapa">Mapa que se valida.</param>
        /// <param name="centro">Posición del Centro Urbano.</param>
        /// <param name="recursos">Recursos físicos que se validan.</param>
        /// <param name="posicionesPorMapa">Registro de posiciones reservadas por las validaciones de ambos participantes.</param>
        /// <exception cref="ArgumentNullException">El mapa, el centro o la lista de recursos es nulo.</exception>
        /// <exception cref="ArgumentException">Hay recursos nulos, posiciones no disponibles o repetidas, o falta oro, madera o comida.</exception>
        private void ValidarMapa(
            Mapa mapa,
            Coordenada centro,
            IReadOnlyList<Recurso> recursos,
            Dictionary<Mapa, HashSet<(int, int)>> posicionesPorMapa)
        {
            if (mapa == null)
            {
                throw new ArgumentNullException(nameof(mapa));
            }

            if (centro == null)
            {
                throw new ArgumentNullException(nameof(centro));
            }

            if (recursos == null)
            {
                throw new ArgumentNullException(nameof(recursos));
            }

            if (!mapa.PuedeColocar(centro))
            {
                throw new ArgumentException("La posición del Centro Urbano no está disponible.", nameof(centro));
            }

            if (!posicionesPorMapa.TryGetValue(mapa, out var posiciones))
            {
                posiciones = new HashSet<(int, int)>();
                posicionesPorMapa.Add(mapa, posiciones);
            }

            if (!posiciones.Add((centro.X, centro.Y)))
            {
                throw new ArgumentException("La posición del Centro Urbano está repetida.", nameof(centro));
            }

            bool tieneOro = false;
            bool tieneMadera = false;
            bool tieneComida = false;

            foreach (Recurso recurso in recursos)
            {
                if (recurso == null)
                {
                    throw new ArgumentException("La lista no puede contener recursos nulos.", nameof(recursos));
                }

                if (!mapa.PuedeColocar(recurso.Coordenada))
                {
                    throw new ArgumentException("La posición de un recurso no está disponible.", nameof(recursos));
                }

                if (!posiciones.Add((recurso.Coordenada.X, recurso.Coordenada.Y)))
                {
                    throw new ArgumentException(
                        "Un recurso coincide con un Centro Urbano u otro recurso.", nameof(recursos));
                }

                tieneOro = tieneOro || recurso.Tipo == TipoRecurso.Oro;
                tieneMadera = tieneMadera || recurso.Tipo == TipoRecurso.Madera;
                tieneComida = tieneComida || recurso.Tipo == TipoRecurso.Comida;
            }

            if (!tieneOro || !tieneMadera || !tieneComida)
            {
                throw new ArgumentException(
                    "Se requiere al menos un recurso de Oro, Madera y Comida.", nameof(recursos));
            }
        }

        /// <summary>
        /// Ocupa la casilla del centro, añade el Centro Urbano al jugador y coloca los recursos previamente validados.
        /// </summary>
        /// <param name="jugador">Jugador cuyo mapa se configura.</param>
        /// <param name="centro">Posición validada del Centro Urbano.</param>
        /// <param name="recursos">Recursos físicos previamente validados.</param>
        /// <exception cref="InvalidOperationException">No se logra ocupar la casilla del centro o colocar uno de los recursos.</exception>
        private void ConfigurarMapa(
            Jugador jugador,
            Coordenada centro,
            IReadOnlyList<Recurso> recursos)
        {
            if (!jugador.Mapa.ObtenerCasilla(centro.X, centro.Y).Ocupar())
            {
                throw new InvalidOperationException("No se pudo ocupar la casilla del Centro Urbano.");
            }

            jugador.AgregarEdificio(new CentroUrbano(centro));

            foreach (Recurso recurso in recursos)
            {
                if (!jugador.Mapa.ColocarRecurso(recurso))
                {
                    throw new InvalidOperationException("No se pudo colocar un recurso validado.");
                }
            }
        }
    }
}
