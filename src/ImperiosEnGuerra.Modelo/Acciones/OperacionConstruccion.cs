using System;
using System.Linq;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    public sealed class OperacionConstruccion
    {
        public ResultadoAccion Ejecutar(
            Partida partida,
            SolicitudConstruccion solicitud)
        {
            if (partida == null)
                return ResultadoAccion.Fallido(
                    "No hay una partida activa.");

            if (solicitud == null)
                return ResultadoAccion.Fallido(
                    "La solicitud de construcción es obligatoria.");

            if (partida.JugadorMaquina.Unidades.Any(
                u => u.Id == solicitud.AldeanoId))
            {
                return ResultadoAccion.Fallido(
                    "No se puede construir con una unidad de la máquina.");
            }

            Unidad unidad = partida.JugadorHumano.Unidades
                .FirstOrDefault(u => u.Id == solicitud.AldeanoId);

            if (unidad == null)
                return ResultadoAccion.Fallido(
                    "No existe una unidad humana con ese ID.");

            if (!(unidad is Aldeano))
                return ResultadoAccion.Fallido(
                    "La unidad seleccionada no es un Aldeano.");

            if (!unidad.Disponible)
                return ResultadoAccion.Fallido(
                    "El Aldeano no está disponible.");

            if (solicitud.Destino == null)
                return ResultadoAccion.Fallido(
                    "La posición de construcción es obligatoria.");

            Mapa mapa = partida.JugadorHumano.Mapa;

            if (!mapa.EstaDentroDeLimites(solicitud.Destino))
                return ResultadoAccion.Fallido(
                    "La posición está fuera del mapa.");

            if (!mapa.PuedeColocar(solicitud.Destino))
                return ResultadoAccion.Fallido(
                    "La posición indicada no está disponible.");

            if (string.IsNullOrWhiteSpace(solicitud.TipoEdificio))
                return ResultadoAccion.Fallido(
                    "El tipo de edificio es obligatorio.");

            if (!string.Equals(
                solicitud.TipoEdificio,
                nameof(CentroUrbano),
                StringComparison.OrdinalIgnoreCase))
            {
                return ResultadoAccion.Fallido(
                    "El tipo de edificio indicado no está permitido.");
            }

            CentroUrbano edificio =
                new CentroUrbano(solicitud.Destino);

            partida.JugadorHumano.AgregarEdificio(edificio);

            Casilla casilla = mapa.ObtenerCasilla(
                solicitud.Destino.X,
                solicitud.Destino.Y);

            if (casilla == null)
                return ResultadoAccion.Fallido(
                    "No se pudo obtener la casilla de construcción.");

            casilla.Ocupar();

            return ResultadoAccion.Exitoso(
                "Construcción realizada correctamente.");
        }
    }
}