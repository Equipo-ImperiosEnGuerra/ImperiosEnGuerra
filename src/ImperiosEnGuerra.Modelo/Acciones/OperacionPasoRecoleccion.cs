using System;
using System.Linq;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recoleccion;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    /// <summary>
    /// Extrae un único ciclo de recurso hacia la carga del Aldeano.
    /// No modifica el saldo económico del jugador.
    /// </summary>
    public sealed class OperacionPasoRecoleccion
    {
        public ResultadoPasoRecoleccion Ejecutar(
            Partida partida,
            Guid aldeanoId,
            Coordenada objetivo,
            int tasa)
        {
            if (partida == null)
            {
                return ResultadoPasoRecoleccion.Fallido(
                    "No hay una partida activa.");
            }

            if (tasa <= 0)
            {
                return ResultadoPasoRecoleccion.Fallido(
                    "La tasa de recolección debe ser positiva.");
            }

            Aldeano aldeano =
                partida.JugadorHumano.Unidades
                    .OfType<Aldeano>()
                    .FirstOrDefault(
                        u => u.Id == aldeanoId);

            if (aldeano == null)
            {
                return ResultadoPasoRecoleccion.Fallido(
                    "No existe un Aldeano humano con ese ID.");
            }

            if (aldeano.OrdenActiva !=
                TipoAccionJuego.Recolectar)
            {
                return ResultadoPasoRecoleccion.Fallido(
                    "El Aldeano no tiene una orden de recolección activa.");
            }

            if (objetivo == null)
            {
                return ResultadoPasoRecoleccion.Fallido(
                    "El objetivo de recolección es obligatorio.");
            }

            Mapa mapa =
                partida.JugadorHumano.Mapa;

            if (!mapa.EstaDentroDeLimites(
                objetivo))
            {
                return ResultadoPasoRecoleccion.Fallido(
                    "El objetivo está fuera del mapa.");
            }

            Recurso recurso =
                mapa.ObtenerRecursoEn(
                    objetivo);

            if (recurso == null)
            {
                return ResultadoPasoRecoleccion.Fallido(
                    "No existe un recurso en la posición indicada.");
            }

            int distancia =
                Math.Abs(
                    aldeano.Coordenada.X -
                    recurso.Coordenada.X)
                +
                Math.Abs(
                    aldeano.Coordenada.Y -
                    recurso.Coordenada.Y);

            if (distancia != 1)
            {
                return ResultadoPasoRecoleccion.Fallido(
                    "El Aldeano debe estar junto al recurso para recolectar.");
            }

            if (recurso.Agotado)
            {
                return ResultadoPasoRecoleccion.Exitoso(
                    0,
                    aldeano.CargaActual,
                    aldeano.CapacidadCarga,
                    true,
                    recurso.Tipo);
            }

            if (aldeano.CapacidadDisponible <= 0)
            {
                return ResultadoPasoRecoleccion.Exitoso(
                    0,
                    aldeano.CargaActual,
                    aldeano.CapacidadCarga,
                    recurso.Agotado,
                    recurso.Tipo);
            }

            if (aldeano.CargaActual > 0 &&
                aldeano.TipoCarga.HasValue &&
                aldeano.TipoCarga.Value != recurso.Tipo)
            {
                return ResultadoPasoRecoleccion.Fallido(
                    "El Aldeano debe depositar su carga actual antes de recolectar otro tipo de recurso.");
            }

            int extraida =
                aldeano.RecolectarDesde(
                    recurso,
                    tasa);

            return ResultadoPasoRecoleccion.Exitoso(
                extraida,
                aldeano.CargaActual,
                aldeano.CapacidadCarga,
                recurso.Agotado,
                recurso.Tipo);
        }
    }
}
