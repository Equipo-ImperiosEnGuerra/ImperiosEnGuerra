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
            SolicitudRecoleccion solicitud)
        {
            ResultadoAccion validacion =
                new OperacionRecoleccion()
                    .Ejecutar(
                        partida,
                        solicitud);

            if (!validacion.Exito)
            {
                return ResultadoAproximacionRecurso.Fallido(
                    validacion.Mensaje);
            }

            Aldeano aldeano =
                partida.JugadorHumano.Unidades
                    .OfType<Aldeano>()
                    .First(
                        u =>
                            u.Id ==
                            solicitud.AldeanoId);

            Mapa mapa =
                partida.JugadorHumano.Mapa;

            Recurso recurso =
                mapa.ObtenerRecursoEn(
                    solicitud.Objetivo);

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
                            candidato));

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
                    "No existe una casilla accesible junto al recurso.");
            }

            return ResultadoAproximacionRecurso.Exitoso(
                recurso.Tipo,
                mejorPunto,
                mejorPlan.Pasos);
        }
    }
}
