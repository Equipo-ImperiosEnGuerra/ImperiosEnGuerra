using System;
using System.Collections.Generic;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;

namespace ImperiosEnGuerra.Modelo.Core
{
    public class InicializadorPartida
    {
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
