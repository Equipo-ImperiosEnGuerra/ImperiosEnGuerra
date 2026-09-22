using System;
using System.Linq;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    public sealed class OperacionEntrenamiento
    {
        public ResultadoAccion Ejecutar(
            Partida partida,
            SolicitudEntrenamiento solicitud)
        {
            if (partida == null)
                return ResultadoAccion.Fallido(
                    "No hay una partida activa.");

            if (solicitud == null)
                return ResultadoAccion.Fallido(
                    "La solicitud de entrenamiento es obligatoria.");

            if (solicitud.EdificioOrigen == null)
                return ResultadoAccion.Fallido(
                    "El edificio de origen es obligatorio.");

            Jugador propietario =
                partida.BuscarJugadorPorEdificio(
                    solicitud.EdificioOrigen);

            if (propietario == null)
            {
                return ResultadoAccion.Fallido(
                    "No existe un edificio propietario único en la posición indicada.");
            }

            return Ejecutar(
                partida,
                solicitud,
                propietario.Tipo);
        }

        public ResultadoAccion Ejecutar(
            Partida partida,
            SolicitudEntrenamiento solicitud,
            TipoJugador propietarioTipo)
        {
            if (partida == null)
                return ResultadoAccion.Fallido(
                    "No hay una partida activa.");

            if (solicitud == null)
                return ResultadoAccion.Fallido(
                    "La solicitud de entrenamiento es obligatoria.");

            if (solicitud.EdificioOrigen == null)
                return ResultadoAccion.Fallido(
                    "El edificio de origen es obligatorio.");

            if (solicitud.Destino == null)
                return ResultadoAccion.Fallido(
                    "La posición de aparición es obligatoria.");

            Jugador propietario =
                partida.ObtenerJugador(
                    propietarioTipo);

            if (propietario == null)
                return ResultadoAccion.Fallido(
                    "No existe el jugador propietario indicado.");

            Edificio edificio =
                propietario.Edificios
                    .FirstOrDefault(
                        e => MismaCoordenada(
                            e.Coordenada,
                            solicitud.EdificioOrigen));

            if (edificio == null)
                return ResultadoAccion.Fallido(
                    "No existe un edificio del jugador indicado en la posición solicitada.");

            if (!(edificio is CentroUrbano))
                return ResultadoAccion.Fallido(
                    "El edificio seleccionado no permite entrenamiento.");

            Mapa mapa =
                propietario.Mapa;

            if (!mapa.EstaDentroDeLimites(
                    solicitud.Destino))
            {
                return ResultadoAccion.Fallido(
                    "La posición de aparición está fuera del mapa.");
            }

            if (!mapa.PuedeColocar(
                    solicitud.Destino))
            {
                return ResultadoAccion.Fallido(
                    "La posición de aparición no está disponible.");
            }

            Unidad unidad =
                FabricaUnidades.Crear(
                    solicitud.TipoUnidad,
                    solicitud.Destino);

            if (unidad == null)
                return ResultadoAccion.Fallido(
                    "El tipo de unidad indicado no está permitido.");

            Casilla casilla =
                mapa.ObtenerCasilla(
                    solicitud.Destino.X,
                    solicitud.Destino.Y);

            if (casilla == null ||
                !casilla.Ocupar())
            {
                return ResultadoAccion.Fallido(
                    "No se pudo ocupar la posición de aparición.");
            }

            propietario.AgregarUnidad(
                unidad);

            return ResultadoAccion.Exitoso(
                $"Unidad {unidad.GetType().Name} entrenada correctamente.");
        }

        private static bool MismaCoordenada(
            Coordenada a,
            Coordenada b)
        {
            return a != null &&
                   b != null &&
                   a.X == b.X &&
                   a.Y == b.Y;
        }
    }
}
