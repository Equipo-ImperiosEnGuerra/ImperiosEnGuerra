using System.Collections.Generic;
using System.Linq;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Movimiento
{
    /// <summary>
    /// Valida una solicitud y calcula su ruta lógica sin cambiar la posición de la unidad.
    /// </summary>
    public sealed class PlanificadorMovimiento
    {
        private readonly BuscadorRutaAStar buscador;

        public PlanificadorMovimiento()
            : this(new BuscadorRutaAStar())
        {
        }

        public PlanificadorMovimiento(
            BuscadorRutaAStar buscador)
        {
            this.buscador =
                buscador
                ?? throw new System.ArgumentNullException(
                    nameof(buscador));
        }

        public ResultadoPlanMovimiento Preparar(
            Partida partida,
            SolicitudMovimiento solicitud,
            bool permitirOrdenMovimientoActiva = false)
        {
            if (partida == null)
                return ResultadoPlanMovimiento.Fallido(
                    "No hay una partida activa.");

            if (solicitud == null)
                return ResultadoPlanMovimiento.Fallido(
                    "La solicitud de movimiento es obligatoria.");

            Jugador propietario =
                partida.BuscarJugadorPorUnidad(
                    solicitud.UnidadId);

            if (propietario == null)
                return ResultadoPlanMovimiento.Fallido(
                    "No existe una unidad con ese ID.");

            Unidad unidad =
                propietario.Unidades
                    .First(
                        u => u.Id == solicitud.UnidadId);

            if (!unidad.Disponible &&
                !(permitirOrdenMovimientoActiva &&
                  unidad.OrdenActiva == TipoAccionJuego.Mover))
            {
                return ResultadoPlanMovimiento.Fallido(
                    "La unidad no está disponible.");
            }

            Coordenada destino = solicitud.Destino;
            Mapa mapa = propietario.Mapa;

            if (destino == null)
                return ResultadoPlanMovimiento.Fallido(
                    "El destino es obligatorio.");

            if (!mapa.EstaDentroDeLimites(destino))
                return ResultadoPlanMovimiento.Fallido(
                    "El destino está fuera del mapa.");

            Casilla casilla =
                mapa.ObtenerCasilla(
                    destino.X,
                    destino.Y);

            if (!casilla.EsTransitable)
                return ResultadoPlanMovimiento.Fallido(
                    "La casilla destino no es transitable.");

            if (casilla.EstaOcupada)
                return ResultadoPlanMovimiento.Fallido(
                    "La casilla destino está ocupada.");

            if (mapa.ObtenerRecursoEn(destino) != null)
                return ResultadoPlanMovimiento.Fallido(
                    "La casilla destino contiene un recurso físico.");

            if (partida.Jugadores.Any(
                    jugador =>
                        ReferenceEquals(
                            jugador.Mapa,
                            mapa) &&
                        TieneEntidadEn(
                            jugador,
                            unidad,
                            destino)))
            {
                return ResultadoPlanMovimiento.Fallido(
                    "La posición destino contiene una unidad o un edificio.");
            }

            List<Coordenada> bloqueos =
                ObtenerBloqueos(
                    partida,
                    mapa,
                    unidad,
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
                                unidad))
                    .Select(
                        u => u.Coordenada)
                    .Where(
                        coordenada =>
                            coordenada != null)
                    .ToList();

            ResultadoRuta ruta =
                buscador.Buscar(
                    mapa,
                    unidad.Coordenada,
                    destino,
                    bloqueos,
                    ocupacionesAliadas);

            if (!ruta.Encontrada)
            {
                return ResultadoPlanMovimiento.Fallido(
                    "No existe una ruta transitable hasta el destino.");
            }

            return ResultadoPlanMovimiento.Exitoso(
                ruta.Pasos);
        }

        private static List<Coordenada> ObtenerBloqueos(
            Partida partida,
            Mapa mapa,
            Unidad unidadMovil,
            Jugador propietario)
        {
            var bloqueos =
                new List<Coordenada>();

            foreach (Jugador jugador in partida.Jugadores)
            {
                AgregarBloqueos(
                    jugador,
                    mapa,
                    unidadMovil,
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
            Unidad unidadMovil,
            bool esAliado,
            List<Coordenada> bloqueos)
        {
            if (!ReferenceEquals(
                jugador.Mapa,
                mapa))
            {
                return;
            }

            // Las unidades aliadas son obstáculos dinámicos: no deben cerrar
            // permanentemente una ruta de A*. La aplicación paso a paso
            // mantiene las reglas de colisión con enemigos y estructuras.
            if (!esAliado)
            {
                foreach (Unidad unidad in jugador.Unidades)
                {
                    if (!ReferenceEquals(
                        unidad,
                        unidadMovil))
                    {
                        bloqueos.Add(
                            unidad.Coordenada);
                    }
                }
            }

            // Los edificios continúan siendo obstáculos duros,
            // independientemente del propietario.
            foreach (var edificio in jugador.Edificios)
            {
                bloqueos.Add(
                    edificio.Coordenada);
            }
        }

        private static bool TieneEntidadEn(
            Jugador jugador,
            Unidad unidadMovil,
            Coordenada destino)
        {
            return jugador.Unidades.Any(
                       u =>
                           !ReferenceEquals(
                               u,
                               unidadMovil) &&
                           Coincide(
                               u.Coordenada,
                               destino))
                ||
                   jugador.Edificios.Any(
                       e => Coincide(
                           e.Coordenada,
                           destino));
        }

        private static bool Coincide(
            Coordenada posicion,
            Coordenada destino)
        {
            return posicion != null &&
                   posicion.X == destino.X &&
                   posicion.Y == destino.Y;
        }
    }
}
