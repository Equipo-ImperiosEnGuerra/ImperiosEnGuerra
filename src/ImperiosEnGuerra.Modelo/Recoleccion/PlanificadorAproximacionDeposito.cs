using System;
using System.Linq;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Movimiento;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Recoleccion
{
    /// <summary>
    /// Selecciona el Centro Urbano humano accesible más cercano y una casilla
    /// ortogonal adyacente desde la cual el Aldeano puede depositar.
    /// </summary>
    public sealed class PlanificadorAproximacionDeposito
    {
        private static readonly (int X, int Y)[] Direcciones =
        {
            (1, 0),
            (-1, 0),
            (0, 1),
            (0, -1)
        };

        private readonly PlanificadorMovimiento planificadorMovimiento;

        public PlanificadorAproximacionDeposito()
            : this(new PlanificadorMovimiento())
        {
        }

        public PlanificadorAproximacionDeposito(
            PlanificadorMovimiento planificadorMovimiento)
        {
            this.planificadorMovimiento =
                planificadorMovimiento
                ?? throw new ArgumentNullException(
                    nameof(planificadorMovimiento));
        }

        public ResultadoAproximacionDeposito Preparar(
            Partida partida,
            Guid aldeanoId,
            bool permitirOrdenMovimientoActiva = false)
        {
            if (partida == null)
            {
                return ResultadoAproximacionDeposito.Fallido(
                    "No hay una partida activa.");
            }

            Aldeano aldeano =
                partida.JugadorHumano.Unidades
                    .OfType<Aldeano>()
                    .FirstOrDefault(
                        u => u.Id == aldeanoId);

            if (aldeano == null)
            {
                return ResultadoAproximacionDeposito.Fallido(
                    "No existe un Aldeano humano con ese ID.");
            }

            if (!aldeano.Disponible &&
                !(permitirOrdenMovimientoActiva &&
                  aldeano.OrdenActiva == TipoAccionJuego.Mover))
            {
                return ResultadoAproximacionDeposito.Fallido(
                    "El Aldeano no está disponible.");
            }

            CentroUrbano[] centros =
                partida.JugadorHumano.Edificios
                    .OfType<CentroUrbano>()
                    .ToArray();

            if (centros.Length == 0)
            {
                return ResultadoAproximacionDeposito.Fallido(
                    "No existe un Centro Urbano humano para depositar.");
            }

            Mapa mapa =
                partida.JugadorHumano.Mapa;

            ResultadoPlanMovimiento mejorPlan = null;
            Coordenada mejorCentro = null;
            Coordenada mejorPunto = null;

            foreach (CentroUrbano centro in centros)
            {
                foreach ((int X, int Y) direccion
                         in Direcciones)
                {
                    Coordenada candidato =
                        new Coordenada(
                            centro.Coordenada.X + direccion.X,
                            centro.Coordenada.Y + direccion.Y);

                    if (!mapa.EstaDentroDeLimites(candidato))
                    {
                        continue;
                    }

                    ResultadoPlanMovimiento plan =
                        planificadorMovimiento.Preparar(
                            partida,
                            new SolicitudMovimiento(
                                aldeano.Id,
                                candidato),
                            permitirOrdenMovimientoActiva);

                    if (!plan.Exito)
                    {
                        continue;
                    }

                    if (mejorPlan == null ||
                        plan.Pasos.Count < mejorPlan.Pasos.Count)
                    {
                        mejorPlan = plan;
                        mejorCentro = centro.Coordenada;
                        mejorPunto = candidato;
                    }
                }
            }

            if (mejorPlan == null)
            {
                return ResultadoAproximacionDeposito.Fallido(
                    "No existe una ruta accesible hasta un Centro Urbano.",
                    true);
            }

            return ResultadoAproximacionDeposito.Exitoso(
                mejorCentro,
                mejorPunto,
                mejorPlan.Pasos);
        }
    }
}
