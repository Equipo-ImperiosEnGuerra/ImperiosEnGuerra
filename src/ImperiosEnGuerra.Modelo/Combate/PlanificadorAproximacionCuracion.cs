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
    /// Calcula una posición alcanzable desde la que un Monje pueda curar
    /// a una unidad aliada. No modifica el Modelo.
    /// </summary>
    public sealed class PlanificadorAproximacionCuracion
    {
        private readonly BuscadorRutaAStar buscador =
            new BuscadorRutaAStar();

        public ResultadoAproximacionCuracion Preparar(
            Partida partida,
            SolicitudCuracion solicitud)
        {
            if (partida == null)
            {
                return ResultadoAproximacionCuracion.Fallido(
                    "No hay una partida activa.");
            }

            if (solicitud == null)
            {
                return ResultadoAproximacionCuracion.Fallido(
                    "La solicitud de curación es obligatoria.");
            }

            Jugador propietario =
                partida.BuscarJugadorPorUnidad(
                    solicitud.CuradorId);

            if (propietario == null)
            {
                return ResultadoAproximacionCuracion.Fallido(
                    "No existe la unidad curadora indicada.");
            }

            Monje curador =
                propietario.Unidades
                    .OfType<Monje>()
                    .FirstOrDefault(
                        u => u.Id == solicitud.CuradorId);

            if (curador == null)
            {
                return ResultadoAproximacionCuracion.Fallido(
                    "Solo un Monje puede ejecutar la acción Curar.");
            }

            if (solicitud.CuradorId == solicitud.ObjetivoId)
            {
                return ResultadoAproximacionCuracion.Fallido(
                    "El Monje no puede curarse a sí mismo.");
            }

            Unidad objetivo =
                propietario.Unidades
                    .FirstOrDefault(
                        u => u.Id == solicitud.ObjetivoId);

            if (objetivo == null)
            {
                return ResultadoAproximacionCuracion.Fallido(
                    "La curación solo puede aplicarse a una unidad aliada.");
            }

            if (objetivo.Destruida)
            {
                return ResultadoAproximacionCuracion.Fallido(
                    "No se puede curar una unidad destruida.");
            }

            if (objetivo.VidaActual >= objetivo.VidaMaxima)
            {
                return ResultadoAproximacionCuracion.Fallido(
                    "La unidad aliada ya tiene la vida completa.");
            }

            if (!ReferenceEquals(
                    propietario.Mapa,
                    partida.BuscarJugadorPorUnidad(
                        solicitud.ObjetivoId)?.Mapa))
            {
                return ResultadoAproximacionCuracion.Fallido(
                    "El Monje y el aliado no comparten el mismo mapa lógico.");
            }

            if (Distancia(
                    curador.Coordenada,
                    objetivo.Coordenada) <=
                curador.AlcanceCuracion)
            {
                return ResultadoAproximacionCuracion.EnAlcance(
                    curador.Coordenada);
            }

            Mapa mapa =
                propietario.Mapa;

            List<Coordenada> bloqueos =
                ObtenerBloqueos(
                    partida,
                    mapa,
                    curador,
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
                                curador))
                    .Select(
                        u => u.Coordenada)
                    .Where(
                        coordenada =>
                            coordenada != null)
                    .ToList();

            var candidatas =
                new List<(Coordenada Punto, IReadOnlyList<Coordenada> Pasos)>();

            for (int x = 0;
                 x < mapa.Ancho;
                 x++)
            {
                for (int y = 0;
                     y < mapa.Alto;
                     y++)
                {
                    var candidata =
                        new Coordenada(
                            x,
                            y);

                    int distanciaObjetivo =
                        Distancia(
                            candidata,
                            objetivo.Coordenada);

                    if (distanciaObjetivo <= 0 ||
                        distanciaObjetivo >
                            curador.AlcanceCuracion)
                    {
                        continue;
                    }

                    if (!mapa.PuedeColocar(
                            candidata) ||
                        HayEntidadEn(
                            partida,
                            mapa,
                            curador,
                            candidata))
                    {
                        continue;
                    }

                    ResultadoRuta ruta =
                        buscador.Buscar(
                            mapa,
                            curador.Coordenada,
                            candidata,
                            bloqueos,
                            ocupacionesAliadas);

                    if (!ruta.Encontrada)
                        continue;

                    candidatas.Add(
                        (
                            candidata,
                            ruta.Pasos
                        ));
                }
            }

            if (candidatas.Count == 0)
            {
                return ResultadoAproximacionCuracion.Fallido(
                    "No existe una ruta transitable hasta una posición válida de curación.");
            }

            var mejor =
                candidatas
                    .OrderBy(
                        c => c.Pasos.Count)
                    .ThenBy(
                        c => DistanciaManhattan(
                            curador.Coordenada,
                            c.Punto))
                    .ThenBy(
                        c => c.Punto.X)
                    .ThenBy(
                        c => c.Punto.Y)
                    .First();

            return ResultadoAproximacionCuracion.Exitoso(
                mejor.Punto,
                mejor.Pasos);
        }

        private static List<Coordenada> ObtenerBloqueos(
            Partida partida,
            Mapa mapa,
            Unidad curador,
            Jugador propietario)
        {
            var bloqueos =
                new List<Coordenada>();

            foreach (Jugador jugador in partida.Jugadores)
            {
                AgregarBloqueos(
                    jugador,
                    mapa,
                    curador,
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
            Unidad curador,
            bool esAliado,
            List<Coordenada> bloqueos)
        {
            if (!ReferenceEquals(
                    jugador.Mapa,
                    mapa))
            {
                return;
            }

            if (!esAliado)
            {
                foreach (Unidad unidad
                         in jugador.Unidades)
                {
                    if (!ReferenceEquals(
                            unidad,
                            curador))
                    {
                        bloqueos.Add(
                            unidad.Coordenada);
                    }
                }
            }

            foreach (Edificio edificio
                     in jugador.Edificios)
            {
                bloqueos.Add(
                    edificio.Coordenada);
            }
        }

        private static bool HayEntidadEn(
            Partida partida,
            Mapa mapa,
            Unidad curador,
            Coordenada posicion)
        {
            return partida.Jugadores.Any(
                jugador =>
                    TieneEntidad(
                        jugador,
                        mapa,
                        curador,
                        posicion));
        }

        private static bool TieneEntidad(
            Jugador jugador,
            Mapa mapa,
            Unidad curador,
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
                               curador) &&
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

        private static int Distancia(
            Coordenada a,
            Coordenada b)
        {
            return Math.Max(
                Math.Abs(
                    a.X - b.X),
                Math.Abs(
                    a.Y - b.Y));
        }

        private static int DistanciaManhattan(
            Coordenada a,
            Coordenada b)
        {
            return Math.Abs(
                       a.X - b.X) +
                   Math.Abs(
                       a.Y - b.Y);
        }
    }
}
