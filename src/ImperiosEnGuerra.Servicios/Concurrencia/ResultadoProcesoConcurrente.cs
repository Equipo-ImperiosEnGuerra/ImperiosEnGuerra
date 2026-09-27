using System;
using ImperiosEnGuerra.Modelo.Acciones;

namespace ImperiosEnGuerra.Servicios.Concurrencia
{
    /// <summary>
    /// Estado final que puede producir un proceso ejecutado en segundo plano.
    /// </summary>
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
        //Conserva el hilo usado por el worker para poder demostrar la ejecución concurrente.
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

        //Crea el mensaje que se publica cuando el trabajo termina normalmente.
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

        //Crea el mensaje que informa una cancelación solicitada.
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

        //Crea el mensaje que conserva el error técnico sin lanzarlo hacia el consumidor.
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
