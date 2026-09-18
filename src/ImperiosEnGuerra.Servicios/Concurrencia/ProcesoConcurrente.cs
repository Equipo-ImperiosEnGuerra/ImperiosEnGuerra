using System;
using System.Threading.Tasks;

namespace ImperiosEnGuerra.Servicios.Concurrencia
{
    /// <summary>
    /// Referencia estable a un proceso concurrente iniciado.
    /// </summary>
    public sealed class ProcesoConcurrente
    {
        public Guid Id { get; }
        public string Nombre { get; }
        public Task Finalizacion { get; }

        internal ProcesoConcurrente(
            Guid id,
            string nombre,
            Task finalizacion)
        {
            Id = id;
            Nombre = nombre;
            Finalizacion = finalizacion
                ?? throw new ArgumentNullException(nameof(finalizacion));
        }
    }
}
