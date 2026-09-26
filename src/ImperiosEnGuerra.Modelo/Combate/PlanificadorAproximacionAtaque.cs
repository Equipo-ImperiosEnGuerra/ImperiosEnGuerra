using System;
using System.Collections.Generic;
using System.Linq;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Movimiento;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Combate
{
    /// <summary>
    /// Calcula una posición alcanzable desde la que una unidad pueda atacar.
    /// No modifica el Modelo.
    /// </summary>
    public sealed class PlanificadorAproximacionAtaque
    {
        private readonly BuscadorRutaAStar buscador =
            new BuscadorRutaAStar();

        public ResultadoAproximacionAtaque Preparar(
            Partida partida,
            SolicitudAtaque solicitud)
        {
            if (partida == null)
                return ResultadoAproximacionAtaque.Fallido(
                    "No hay una partida activa.");

            if (solicitud == null)
                return ResultadoAproximacionAtaque.Fallido(
                    "La solicitud de ataque es obligatoria.");

            Jugador propietario =
                partida.BuscarJugadorPorUnidad(
                    solicitud.AtacanteId);

            if (propietario == null)
                return ResultadoAproximacionAtaque.Fallido(
                    "No existe la unidad atacante indicada.");

            Unidad atacante =
                propietario.Unidades.First(
                    u => u.Id == solicitud.AtacanteId);

            if (atacante.AlcanceAtaque <= 0)
                return ResultadoAproximacionAtaque.Fallido(
                    "La unidad atacante no tiene alcance ofensivo.");

            Jugador objetivoPropietario =
                partida.BuscarJugadorPorUnidad(
                    solicitud.ObjetivoId)
                ??
                partida.BuscarJugadorPorEdificio(
                    solicitud.ObjetivoId);

            if (objetivoPropietario == null ||
                !partida.SonEnemigos(
                    propietario,
                    objetivoPropietario) ||
                !ReferenceEquals(
                    propietario.Mapa,
                    objetivoPropietario.Mapa))
            {
                return ResultadoAproximacionAtaque.Fallido(
                    "El objetivo no pertenece a una facción enemiga válida.");
            }

            Unidad objetivoUnidad =
                objetivoPropietario.Unidades.FirstOrDefault(
                    u => u.Id == solicitud.ObjetivoId);

            Edificio objetivoEdificio =
                objetivoPropietario.Edificios.FirstOrDefault(
                    e => e.Id == solicitud.ObjetivoId);

            Coordenada objetivo =
                objetivoUnidad?.Coordenada ??
                objetivoEdificio?.Coordenada;

            if (objetivo == null)
                return ResultadoAproximacionAtaque.Fallido(
                    "No existe la entidad enemiga objetivo indicada.");

            if (DistanciaCombate(
                    atacante.Coordenada,
                    objetivo) <= atacante.AlcanceAtaque)
            {
                return ResultadoAproximacionAtaque.EnAlcance(
                    atacante.Coordenada);
            }

            Mapa mapa =
                propietario.Mapa;

            List<Coordenada> bloqueos =
                ObtenerBloqueos(
                    partida,
                    mapa,
                    atacante,
                    propietario);

            List<Coordenada> ocupacionesAliadas =
                partida.Jugadores
                    .Where(
                        jugador =>
                            partida.SonAliados(
                                propietario,
                                jugador))
                    .SelectMany(
                        jugador =>
                            jugador.Unidades)
                    .Where(
                        u =>
                            !ReferenceEquals(
                                u,
                                atacante))
                    .Select(
                        u => u.Coordenada)
                    .Where(
                        coordenada =>
                            coordenada != null)
                    .ToList();

            var candidatas =
                new List<(Coordenada Punto, IReadOnlyList<Coordenada> Pasos)>();

            for (int x = 0; x < mapa.Ancho; x++)
            {
                for (int y = 0; y < mapa.Alto; y++)
                {
                    var candidata =
                        new Coordenada(x, y);

                    int distanciaObjetivo =
                        DistanciaCombate(
                            candidata,
                            objetivo);

                    if (distanciaObjetivo <= 0 ||
                        distanciaObjetivo > atacante.AlcanceAtaque)
                    {
                        continue;
                    }

                    if (!mapa.PuedeColocar(candidata) ||
                        HayEntidadEn(
                            partida,
                            mapa,
                            atacante,
                            candidata))
                    {
                        continue;
                    }

                    ResultadoRuta ruta =
                        buscador.Buscar(
                            mapa,
                            atacante.Coordenada,
                            candidata,
                            bloqueos,
                            ocupacionesAliadas);

                    if (!ruta.Encontrada)
                        continue;

                    candidatas.Add(
                        (candidata, ruta.Pasos));
                }
            }

            if (candidatas.Count == 0)
            {
                return ResultadoAproximacionAtaque.Fallido(
                    "No existe una ruta transitable hasta una posición de ataque válida.");
            }

            var mejor =
                candidatas
                    .OrderBy(c => c.Pasos.Count)
                    .ThenBy(c => DistanciaManhattan(
                        atacante.Coordenada,
                        c.Punto))
                    .ThenBy(c => c.Punto.X)
                    .ThenBy(c => c.Punto.Y)
                    .First();

            return ResultadoAproximacionAtaque.Exitoso(
                mejor.Punto,
                mejor.Pasos);
        }

        private static List<Coordenada> ObtenerBloqueos(
            Partida partida,
            Mapa mapa,
            Unidad atacante,
            Jugador propietario)
        {
            var bloqueos =
                new List<Coordenada>();

            foreach (Jugador jugador in partida.Jugadores)
            {
                AgregarBloqueos(
                    jugador,
                    mapa,
                    atacante,
                    partida.SonAliados(
                        propietario,
                        jugador),
                    bloqueos);
            }

            return bloqueos;
        }

        private static void AgregarBloqueos(
            Jugador jugador,
            Mapa mapa,
            Unidad atacante,
            bool esAliado,
            List<Coordenada> bloqueos)
        {
            if (!ReferenceEquals(
                    jugador.Mapa,
                    mapa))
            {
                return;
            }

            // Los compañeros de equipo no cierran la ruta de aproximación.
            // En cambio las unidades enemigas continúan siendo obstáculos.
            if (!esAliado)
            {
                foreach (Unidad unidad in jugador.Unidades)
                {
                    if (!ReferenceEquals(
                            unidad,
                            atacante))
                    {
                        bloqueos.Add(
                            unidad.Coordenada);
                    }
                }
            }

            foreach (Edificio edificio in jugador.Edificios)
            {
                bloqueos.Add(
                    edificio.Coordenada);
            }
        }

        private static bool HayEntidadEn(
            Partida partida,
            Mapa mapa,
            Unidad atacante,
            Coordenada posicion)
        {
            return partida.Jugadores.Any(
                jugador =>
                    TieneEntidad(
                        jugador,
                        mapa,
                        atacante,
                        posicion));
        }

        private static bool TieneEntidad(
            Jugador jugador,
            Mapa mapa,
            Unidad atacante,
            Coordenada posicion)
        {
            if (!ReferenceEquals(
                    jugador.Mapa,
                    mapa))
            {
                return false;
            }

            return jugador.Unidades.Any(
                       u =>
                           !ReferenceEquals(
                               u,
                               atacante) &&
                           Coincide(
                               u.Coordenada,
                               posicion))
                   ||
                   jugador.Edificios.Any(
                       e => Coincide(
                           e.Coordenada,
                           posicion));
        }

        private static bool Coincide(
            Coordenada a,
            Coordenada b)
        {
            return a != null &&
                   b != null &&
                   a.X == b.X &&
                   a.Y == b.Y;
        }

        private static int DistanciaCombate(
            Coordenada a,
            Coordenada b)
        {
            return Math.Max(
                Math.Abs(a.X - b.X),
                Math.Abs(a.Y - b.Y));
        }

        private static int DistanciaManhattan(
            Coordenada a,
            Coordenada b)
        {
            return Math.Abs(a.X - b.X) +
                   Math.Abs(a.Y - b.Y);
        }
    }
}
