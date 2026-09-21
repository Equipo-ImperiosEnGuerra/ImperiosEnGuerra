using System;
using System.Linq;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Movimiento;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Edificios
{
    /// <summary>
    /// Calcula una ruta hasta una casilla libre adyacente a una obra reservada.
    /// </summary>
    public sealed class PlanificadorAproximacionConstruccion
    {
        private static readonly (int X, int Y)[] Direcciones =
        {
            (1, 0),
            (-1, 0),
            (0, 1),
            (0, -1)
        };

        private readonly PlanificadorMovimiento planificadorMovimiento;

        public PlanificadorAproximacionConstruccion()
            : this(new PlanificadorMovimiento())
        {
        }

        public PlanificadorAproximacionConstruccion(
            PlanificadorMovimiento planificadorMovimiento)
        {
            this.planificadorMovimiento =
                planificadorMovimiento
                ?? throw new ArgumentNullException(
                    nameof(planificadorMovimiento));
        }

        public ResultadoAproximacionConstruccion Preparar(
            Partida partida,
            Guid aldeanoId,
            Coordenada obra,
            bool permitirOrdenMovimientoActiva = false)
        {
            if (partida == null)
            {
                return ResultadoAproximacionConstruccion.Fallido(
                    "No hay una partida activa.");
            }

            Aldeano aldeano =
                partida.JugadorHumano.Unidades
                    .OfType<Aldeano>()
                    .FirstOrDefault(
                        u => u.Id == aldeanoId);

            if (aldeano == null)
            {
                return ResultadoAproximacionConstruccion.Fallido(
                    "No existe un Aldeano humano con ese ID.");
            }

            if (!aldeano.Disponible &&
                !(permitirOrdenMovimientoActiva &&
                  aldeano.OrdenActiva == TipoAccionJuego.Mover))
            {
                return ResultadoAproximacionConstruccion.Fallido(
                    "El Aldeano no está disponible.");
            }

            if (obra == null)
            {
                return ResultadoAproximacionConstruccion.Fallido(
                    "La posición de obra es obligatoria.");
            }

            Mapa mapa =
                partida.JugadorHumano.Mapa;

            ResultadoPlanMovimiento mejorPlan = null;
            Coordenada mejorPunto = null;

            foreach ((int X, int Y) direccion in Direcciones)
            {
                Coordenada candidato =
                    new Coordenada(
                        obra.X + direccion.X,
                        obra.Y + direccion.Y);

                if (!mapa.EstaDentroDeLimites(candidato))
                    continue;

                ResultadoPlanMovimiento plan =
                    planificadorMovimiento.Preparar(
                        partida,
                        new SolicitudMovimiento(
                            aldeano.Id,
                            candidato),
                        permitirOrdenMovimientoActiva);

                if (!plan.Exito)
                    continue;

                if (mejorPlan == null ||
                    plan.Pasos.Count < mejorPlan.Pasos.Count)
                {
                    mejorPlan = plan;
                    mejorPunto = candidato;
                }
            }

            if (mejorPlan == null)
            {
                return ResultadoAproximacionConstruccion.Fallido(
                    "No existe una casilla accesible junto a la obra.",
                    true);
            }

            return ResultadoAproximacionConstruccion.Exitoso(
                mejorPunto,
                mejorPlan.Pasos);
        }
    }
}
