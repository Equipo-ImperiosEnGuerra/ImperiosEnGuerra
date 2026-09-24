using System;
using System.Linq;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    /// <summary>
    /// Aplica exactamente un paso ortogonal de una orden de movimiento ya iniciada.
    /// </summary>
    public sealed class OperacionPasoMovimiento
    {
        public ResultadoAccion Ejecutar(
            Partida partida,
            Guid unidadId,
            Coordenada siguiente)
        {
            if (partida == null)
                return ResultadoAccion.Fallido(
                    "No hay una partida activa.");

            Jugador propietario =
                partida.BuscarJugadorPorUnidad(
                    unidadId);

            if (propietario == null)
                return ResultadoAccion.Fallido(
                    "No existe una unidad con ese ID.");

            Unidad unidad =
                propietario.Unidades
                    .First(
                        u => u.Id == unidadId);

            if (unidad.OrdenActiva != TipoAccionJuego.Mover)
            {
                return ResultadoAccion.Fallido(
                    "La unidad no tiene una orden de movimiento activa.");
            }

            if (siguiente == null)
                return ResultadoAccion.Fallido(
                    "El siguiente paso es obligatorio.");

            Mapa mapa =
                propietario.Mapa;

            if (!mapa.EstaDentroDeLimites(
                siguiente))
            {
                return ResultadoAccion.Fallido(
                    "El paso está fuera del mapa.");
            }

            int distancia =
                Math.Abs(
                    siguiente.X -
                    unidad.Coordenada.X)
                +
                Math.Abs(
                    siguiente.Y -
                    unidad.Coordenada.Y);

            if (distancia != 1)
            {
                return ResultadoAccion.Fallido(
                    "El movimiento progresivo solo puede avanzar una casilla ortogonal por paso.");
            }

            Casilla casilla =
                mapa.ObtenerCasilla(
                    siguiente.X,
                    siguiente.Y);

            if (!casilla.EsTransitable)
                return ResultadoAccion.Fallido(
                    "El paso no es transitable.");

            if (mapa.ObtenerRecursoEn(
                    siguiente) != null)
            {
                return ResultadoAccion.Fallido(
                    "El paso contiene un recurso físico.");
            }

            Jugador oponente =
                partida.ObtenerOponente(
                    propietario);

            bool hayUnidadAliada =
                TieneUnidadEn(
                    propietario,
                    unidad,
                    siguiente);

            bool hayUnidadEnemiga =
                oponente != null &&
                ReferenceEquals(
                    oponente.Mapa,
                    mapa) &&
                TieneUnidadEn(
                    oponente,
                    unidad,
                    siguiente);

            if (HayEdificioEn(
                    partida,
                    mapa,
                    siguiente))
            {
                return ResultadoAccion.Fallido(
                    "El paso contiene un edificio.");
            }

            if (hayUnidadEnemiga)
            {
                return ResultadoAccion.Fallido(
                    "El paso contiene una unidad enemiga.");
            }

            // Una unidad aliada puede ser atravesada de forma temporal.
            // El destino final sigue validándose como libre en el planificador.
            // Esto evita que formaciones propias conviertan el mapa en un
            // laberinto imposible sin permitir atravesar enemigos/edificios.
            if (casilla.EstaOcupada &&
                !hayUnidadAliada)
            {
                return ResultadoAccion.Fallido(
                    "El paso está ocupado.");
            }

            Casilla origen =
                mapa.ObtenerCasilla(
                    unidad.Coordenada.X,
                    unidad.Coordenada.Y);

            // Las unidades entrenadas marcan temporalmente su casilla de
            // aparición. La posición real de unidades se controla por las
            // colecciones de Jugador, por lo que esa marca debe liberarse
            // cuando la unidad abandona el spawn para no dejar obstáculos
            // fantasma permanentes.
            if (origen != null &&
                origen.EstaOcupada &&
                !HayEdificioEn(
                    partida,
                    mapa,
                    unidad.Coordenada))
            {
                origen.Liberar();
            }

            unidad.EstablecerDestino(
                siguiente);

            return ResultadoAccion.Exitoso(
                "Paso de movimiento realizado.");
        }

        private static bool HayEdificioEn(
            Partida partida,
            Mapa mapa,
            Coordenada posicion)
        {
            return TieneEdificio(
                       partida.JugadorHumano,
                       mapa,
                       posicion)
                   ||
                   TieneEdificio(
                       partida.JugadorMaquina,
                       mapa,
                       posicion);
        }

        private static bool TieneEdificio(
            Jugador jugador,
            Mapa mapa,
            Coordenada posicion)
        {
            if (!ReferenceEquals(
                    jugador.Mapa,
                    mapa))
            {
                return false;
            }

            return jugador.Edificios.Any(
                e => Coincide(
                    e.Coordenada,
                    posicion));
        }

        private static bool TieneUnidadEn(
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
