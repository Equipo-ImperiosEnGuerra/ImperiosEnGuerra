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

            if (partida.JugadorMaquina.Unidades.Any(
                u => u.Id == unidadId))
            {
                return ResultadoAccion.Fallido(
                    "No se puede mover una unidad de la máquina.");
            }

            Unidad unidad =
                partida.JugadorHumano.Unidades
                    .FirstOrDefault(
                        u => u.Id == unidadId);

            if (unidad == null)
                return ResultadoAccion.Fallido(
                    "No existe una unidad humana con ese ID.");

            if (unidad.OrdenActiva != TipoAccionJuego.Mover)
            {
                return ResultadoAccion.Fallido(
                    "La unidad no tiene una orden de movimiento activa.");
            }

            if (siguiente == null)
                return ResultadoAccion.Fallido(
                    "El siguiente paso es obligatorio.");

            Mapa mapa =
                partida.JugadorHumano.Mapa;

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

            if (casilla.EstaOcupada)
                return ResultadoAccion.Fallido(
                    "El paso está ocupado.");

            if (mapa.ObtenerRecursoEn(
                    siguiente) != null)
            {
                return ResultadoAccion.Fallido(
                    "El paso contiene un recurso físico.");
            }

            if (TieneEntidadEn(
                    partida.JugadorHumano,
                    unidad,
                    siguiente) ||
                (ReferenceEquals(
                     mapa,
                     partida.JugadorMaquina.Mapa) &&
                 TieneEntidadEn(
                     partida.JugadorMaquina,
                     unidad,
                     siguiente)))
            {
                return ResultadoAccion.Fallido(
                    "El paso contiene una unidad o un edificio.");
            }

            unidad.EstablecerDestino(
                siguiente);

            return ResultadoAccion.Exitoso(
                "Paso de movimiento realizado.");
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
