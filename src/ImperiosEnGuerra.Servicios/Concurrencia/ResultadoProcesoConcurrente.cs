using System;
using ImperiosEnGuerra.Modelo.Acciones;

namespace ImperiosEnGuerra.Servicios.Concurrencia
{
    public enum EstadoProcesoConcurrente
    {
        Completado,
        Cancelado,
        Fallido
    }

    /// <summary>
    /// Mensaje inmutable producido por un worker y listo para ser consumido
    /// desde otra capa, incluido el Main Thread de Unity.
    /// </summary>
    public sealed class ResultadoProcesoConcurrente
    {
        public Guid ProcesoId { get; }
        public string Nombre { get; }
        public EstadoProcesoConcurrente Estado { get; }
        public int HiloTrabajoId { get; }
        public ResultadoAccion? Resultado { get; }
        public string? ErrorTecnico { get; }

        private ResultadoProcesoConcurrente(
            Guid procesoId,
            string nombre,
            EstadoProcesoConcurrente estado,
            int hiloTrabajoId,
            ResultadoAccion? resultado,
            string? errorTecnico)
        {
            ProcesoId = procesoId;
            Nombre = nombre;
            Estado = estado;
            HiloTrabajoId = hiloTrabajoId;
            Resultado = resultado;
            ErrorTecnico = errorTecnico;
        }

        internal static ResultadoProcesoConcurrente Completado(
            Guid procesoId,
            string nombre,
            int hiloTrabajoId,
            ResultadoAccion resultado)
        {
            return new ResultadoProcesoConcurrente(
                procesoId,
                nombre,
                EstadoProcesoConcurrente.Completado,
                hiloTrabajoId,
                resultado,
                null);
        }

        internal static ResultadoProcesoConcurrente Cancelado(
            Guid procesoId,
            string nombre,
            int hiloTrabajoId)
        {
            return new ResultadoProcesoConcurrente(
                procesoId,
                nombre,
                EstadoProcesoConcurrente.Cancelado,
                hiloTrabajoId,
                null,
                null);
        }

        internal static ResultadoProcesoConcurrente Fallido(
            Guid procesoId,
            string nombre,
            int hiloTrabajoId,
            string errorTecnico)
        {
            return new ResultadoProcesoConcurrente(
                procesoId,
                nombre,
                EstadoProcesoConcurrente.Fallido,
                hiloTrabajoId,
                null,
                errorTecnico);
        }
    }
}
