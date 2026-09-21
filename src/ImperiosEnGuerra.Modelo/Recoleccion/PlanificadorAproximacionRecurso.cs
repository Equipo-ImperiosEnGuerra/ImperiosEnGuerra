using System;
using System.Linq;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Movimiento;
using ImperiosEnGuerra.Modelo.Recursos;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Recoleccion
{
    /// <summary>
    /// Busca la mejor casilla transitable adyacente a un recurso y reutiliza
    /// el pathfinding de movimiento para llegar hasta ella.
    /// </summary>
    public sealed class PlanificadorAproximacionRecurso
    {
        private static readonly (int X, int Y)[] Direcciones =
        {
            (1, 0),
            (-1, 0),
            (0, 1),
            (0, -1)
        };

        private readonly PlanificadorMovimiento planificadorMovimiento;

        public PlanificadorAproximacionRecurso()
            : this(new PlanificadorMovimiento())
        {
        }

        public PlanificadorAproximacionRecurso(
            PlanificadorMovimiento planificadorMovimiento)
        {
            this.planificadorMovimiento =
                planificadorMovimiento
                ?? throw new ArgumentNullException(
                    nameof(planificadorMovimiento));
        }

        public ResultadoAproximacionRecurso Preparar(
            Partida partida,
            SolicitudRecoleccion solicitud,
            bool permitirOrdenMovimientoActiva = false)
        {
            if (partida == null)
            {
                return ResultadoAproximacionRecurso.Fallido(
                    "No hay una partida activa.");
            }

            if (solicitud == null)
            {
                return ResultadoAproximacionRecurso.Fallido(
                    "La solicitud de recolección es obligatoria.");
            }

            if (partida.JugadorMaquina.Unidades.Any(
                    u => u.Id == solicitud.AldeanoId))
            {
                return ResultadoAproximacionRecurso.Fallido(
                    "No se puede recolectar con una unidad de la máquina.");
            }

            Aldeano aldeano =
                partida.JugadorHumano.Unidades
                    .OfType<Aldeano>()
                    .FirstOrDefault(
                        u => u.Id == solicitud.AldeanoId);

            if (aldeano == null)
            {
                return ResultadoAproximacionRecurso.Fallido(
                    "No existe un Aldeano humano con ese ID.");
            }

            if (!aldeano.Disponible &&
                !(permitirOrdenMovimientoActiva &&
                  aldeano.OrdenActiva == TipoAccionJuego.Mover))
            {
                return ResultadoAproximacionRecurso.Fallido(
                    "El Aldeano no está disponible.");
            }

            if (solicitud.Objetivo == null)
            {
                return ResultadoAproximacionRecurso.Fallido(
                    "El objetivo de recolección es obligatorio.");
            }

            Mapa mapa =
                partida.JugadorHumano.Mapa;

            if (!mapa.EstaDentroDeLimites(
                    solicitud.Objetivo))
            {
                return ResultadoAproximacionRecurso.Fallido(
                    "El objetivo está fuera del mapa.");
            }

            Recurso recurso =
                mapa.ObtenerRecursoEn(
                    solicitud.Objetivo);

            if (recurso == null)
            {
                return ResultadoAproximacionRecurso.Fallido(
                    "No existe un recurso en la posición indicada.");
            }

            if (!Enum.IsDefined(
                    typeof(TipoRecurso),
                    recurso.Tipo))
            {
                return ResultadoAproximacionRecurso.Fallido(
                    "El objetivo no contiene un tipo de recurso válido.");
            }

            if (recurso.Agotado)
            {
                return ResultadoAproximacionRecurso.Fallido(
                    "El recurso objetivo está agotado.");
            }

            ResultadoPlanMovimiento mejorPlan = null;
            Coordenada mejorPunto = null;

            foreach ((int X, int Y) direccion
                     in Direcciones)
            {
                Coordenada candidato =
                    new Coordenada(
                        recurso.Coordenada.X +
                        direccion.X,
                        recurso.Coordenada.Y +
                        direccion.Y);

                if (!mapa.EstaDentroDeLimites(
                    candidato))
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
                    plan.Pasos.Count <
                    mejorPlan.Pasos.Count)
                {
                    mejorPlan = plan;
                    mejorPunto = candidato;
                }
            }

            if (mejorPlan == null ||
                mejorPunto == null)
            {
                return ResultadoAproximacionRecurso.Fallido(
                    "No existe una casilla accesible junto al recurso.",
                    true);
            }

            return ResultadoAproximacionRecurso.Exitoso(
                recurso.Tipo,
                mejorPunto,
                mejorPlan.Pasos);
        }
    }
}
