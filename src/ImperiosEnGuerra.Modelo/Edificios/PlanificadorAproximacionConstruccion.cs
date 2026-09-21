using System;
using System.Collections.Generic;
using System.Linq;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Movimiento;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Edificios
{
    /// <summary>
    /// Calcula una ruta hasta una casilla libre adyacente a una obra. Las
    /// preferencias por unidad reducen colisiones cuando varios workers
    /// circulan cerca de la misma zona.
    /// </summary>
    public sealed class PlanificadorAproximacionConstruccion
    {
        private const int HolguraRutaPreferida = 2;

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

            if (Distancia(
                    aldeano.Coordenada,
                    obra) == 1)
            {
                return ResultadoAproximacionConstruccion.Exitoso(
                    aldeano.Coordenada,
                    Array.Empty<Coordenada>());
            }

            Mapa mapa =
                partida.JugadorHumano.Mapa;

            var candidatos =
                new List<(int Indice, Coordenada Punto, ResultadoPlanMovimiento Plan)>();

            for (int i = 0;
                 i < Direcciones.Length;
                 i++)
            {
                (int X, int Y) direccion =
                    Direcciones[i];

                Coordenada candidato =
                    new Coordenada(
                        obra.X + direccion.X,
                        obra.Y + direccion.Y);

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

                if (plan.Exito)
                {
                    candidatos.Add(
                        (i, candidato, plan));
                }
            }

            if (candidatos.Count == 0)
            {
                return ResultadoAproximacionConstruccion.Fallido(
                    "No existe una casilla accesible junto a la obra.",
                    true);
            }

            int minimo =
                candidatos.Min(
                    c => c.Plan.Pasos.Count);

            int inicio =
                PreferenciaCasillaInteraccion
                    .ObtenerIndiceInicial(
                        partida,
                        aldeano.Id,
                        Direcciones.Length);

            for (int desplazamiento = 0;
                 desplazamiento < Direcciones.Length;
                 desplazamiento++)
            {
                int indice =
                    (inicio + desplazamiento) %
                    Direcciones.Length;

                var elegido =
                    candidatos
                        .Where(
                            c =>
                                c.Indice == indice &&
                                c.Plan.Pasos.Count <=
                                minimo + HolguraRutaPreferida)
                        .OrderBy(
                            c => c.Plan.Pasos.Count)
                        .FirstOrDefault();

                if (elegido.Plan != null)
                {
                    return ResultadoAproximacionConstruccion.Exitoso(
                        elegido.Punto,
                        elegido.Plan.Pasos);
                }
            }

            var masCorto =
                candidatos
                    .OrderBy(
                        c => c.Plan.Pasos.Count)
                    .First();

            return ResultadoAproximacionConstruccion.Exitoso(
                masCorto.Punto,
                masCorto.Plan.Pasos);
        }

        private static int Distancia(
            Coordenada primera,
            Coordenada segunda)
        {
            return Math.Abs(
                       primera.X -
                       segunda.X)
                   +
                   Math.Abs(
                       primera.Y -
                       segunda.Y);
        }
    }
}
