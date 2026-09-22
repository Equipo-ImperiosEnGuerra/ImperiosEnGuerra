using System;
using System.Linq;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recoleccion;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    /// <summary>
    /// Transfiere la carga transportada al saldo del jugador únicamente cuando
    /// el Aldeano se encuentra junto a un Centro Urbano humano.
    /// </summary>
    public sealed class OperacionDepositoRecoleccion
    {
        public ResultadoDepositoRecoleccion Ejecutar(
            Partida partida,
            Guid aldeanoId,
            Coordenada centroUrbano)
        {
            if (partida == null)
            {
                return ResultadoDepositoRecoleccion.Fallido(
                    "No hay una partida activa.");
            }

            Jugador propietario =
                partida.BuscarJugadorPorUnidad(
                    aldeanoId);

            Aldeano aldeano =
                propietario?.Unidades
                    .OfType<Aldeano>()
                    .FirstOrDefault(
                        u => u.Id == aldeanoId);

            if (propietario == null ||
                aldeano == null)
            {
                return ResultadoDepositoRecoleccion.Fallido(
                    "No existe un Aldeano con ese ID.");
            }

            if (centroUrbano == null)
            {
                return ResultadoDepositoRecoleccion.Fallido(
                    "El Centro Urbano de depósito es obligatorio.");
            }

            CentroUrbano centro =
                propietario.Edificios
                    .OfType<CentroUrbano>()
                    .FirstOrDefault(
                        e =>
                            e.Coordenada.X == centroUrbano.X &&
                            e.Coordenada.Y == centroUrbano.Y);

            if (centro == null)
            {
                return ResultadoDepositoRecoleccion.Fallido(
                    "No existe un Centro Urbano propio en la posición indicada.");
            }

            int distancia =
                Math.Abs(
                    aldeano.Coordenada.X -
                    centro.Coordenada.X)
                +
                Math.Abs(
                    aldeano.Coordenada.Y -
                    centro.Coordenada.Y);

            if (distancia != 1)
            {
                return ResultadoDepositoRecoleccion.Fallido(
                    "El Aldeano debe estar junto al Centro Urbano para depositar.");
            }

            if (aldeano.CargaActual <= 0 ||
                !aldeano.TipoCarga.HasValue)
            {
                return ResultadoDepositoRecoleccion.Fallido(
                    "El Aldeano no transporta recursos para depositar.");
            }

            int cantidad =
                aldeano.VaciarCarga(
                    out var tipo);

            if (cantidad <= 0 ||
                !tipo.HasValue)
            {
                return ResultadoDepositoRecoleccion.Fallido(
                    "No fue posible obtener una carga válida para depositar.");
            }

            propietario.Recursos.Agregar(
                tipo.Value,
                cantidad);

            return ResultadoDepositoRecoleccion.Exitoso(
                cantidad,
                tipo.Value);
        }
    }
}
