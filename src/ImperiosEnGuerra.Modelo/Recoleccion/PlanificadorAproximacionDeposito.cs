using System;
using System.Collections.Generic;
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
    /// Selecciona un Centro Urbano humano accesible y distribuye Aldeanos
    /// entre sus casillas adyacentes para evitar que todos compitan por el
    /// mismo punto de depósito.
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
                return ResultadoAproximacionDeposito.Fallido(
                    "No existe un Aldeano con ese ID.");
            }

            if (!aldeano.Disponible &&
                !(permitirOrdenMovimientoActiva &&
                  aldeano.OrdenActiva == TipoAccionJuego.Mover))
            {
                return ResultadoAproximacionDeposito.Fallido(
                    "El Aldeano no está disponible.");
            }

            CentroUrbano[] centros =
                propietario.Edificios
                    .OfType<CentroUrbano>()
                    .ToArray();

            if (centros.Length == 0)
            {
                return ResultadoAproximacionDeposito.Fallido(
                    "No existe un Centro Urbano propio para depositar.");
            }

            foreach (CentroUrbano centro in centros)
            {
                if (Distancia(
                        aldeano.Coordenada,
                        centro.Coordenada) == 1)
                {
                    return ResultadoAproximacionDeposito.Exitoso(
                        centro.Coordenada,
                        aldeano.Coordenada,
                        Array.Empty<Coordenada>());
                }
            }

            Mapa mapa =
                propietario.Mapa;

            var candidatos =
                new List<(int Indice, Coordenada Centro, Coordenada Punto, ResultadoPlanMovimiento Plan)>();

            foreach (CentroUrbano centro in centros)
            {
                for (int i = 0;
                     i < Direcciones.Length;
                     i++)
                {
                    (int X, int Y) direccion =
                        Direcciones[i];

                    Coordenada candidato =
                        new Coordenada(
                            centro.Coordenada.X + direccion.X,
                            centro.Coordenada.Y + direccion.Y);

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
                            (i, centro.Coordenada, candidato, plan));
                    }
                }
            }

            if (candidatos.Count == 0)
            {
                return ResultadoAproximacionDeposito.Fallido(
                    "No existe una ruta accesible hasta un Centro Urbano.",
                    true);
            }

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
                                c.Indice == indice)
                        .OrderBy(
                            c => c.Plan.Pasos.Count)
                        .FirstOrDefault();

                if (elegido.Plan != null)
                {
                    return ResultadoAproximacionDeposito.Exitoso(
                        elegido.Centro,
                        elegido.Punto,
                        elegido.Plan.Pasos);
                }
            }

            var masCorto =
                candidatos
                    .OrderBy(
                        c => c.Plan.Pasos.Count)
                    .First();

            return ResultadoAproximacionDeposito.Exitoso(
                masCorto.Centro,
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
