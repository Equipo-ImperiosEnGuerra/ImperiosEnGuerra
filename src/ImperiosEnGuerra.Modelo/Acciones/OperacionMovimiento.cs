using System.Linq;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Movimiento;
using ImperiosEnGuerra.Modelo.Unidades;

namespace ImperiosEnGuerra.Modelo.Acciones
{
    /// <summary>
    /// Mantiene el movimiento inmediato legado, pero usa el mismo planificador de rutas del movimiento progresivo.
    /// </summary>
    public sealed class OperacionMovimiento
    {
        public ResultadoAccion Ejecutar(
            Partida partida,
            SolicitudMovimiento solicitud)
        {
            ResultadoPlanMovimiento plan =
                new PlanificadorMovimiento()
                    .Preparar(
                        partida,
                        solicitud);

            if (!plan.Exito)
            {
                return ResultadoAccion.Fallido(
                    plan.Mensaje);
            }

            Jugador propietario =
                partida.BuscarJugadorPorUnidad(
                    solicitud.UnidadId);

            if (propietario == null)
            {
                return ResultadoAccion.Fallido(
                    "No existe una unidad con ese ID.");
            }

            Unidad unidad =
                propietario.Unidades
                    .First(
                        u =>
                            u.Id ==
                            solicitud.UnidadId);

            if (plan.Pasos.Count > 0)
            {
                unidad.EstablecerDestino(
                    solicitud.Destino);
            }

            return ResultadoAccion.Exitoso(
                "Movimiento realizado.");
        }
    }
}
