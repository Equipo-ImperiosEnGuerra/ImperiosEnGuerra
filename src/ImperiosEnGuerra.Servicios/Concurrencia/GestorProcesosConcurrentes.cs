using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using ImperiosEnGuerra.Modelo.Acciones;

namespace ImperiosEnGuerra.Servicios.Concurrencia
{
    /// <summary>
    /// Inicia trabajos reales en ThreadPool mediante Task.Run, permite cancelarlos
    /// y publica sus resultados en una cola thread-safe para consumo posterior.
    /// No depende de UnityEngine.
    /// </summary>
    public sealed class GestorProcesosConcurrentes : IDisposable
    {
        private readonly ConcurrentDictionary<Guid, CancellationTokenSource> cancelaciones =
            new ConcurrentDictionary<Guid, CancellationTokenSource>();

        private readonly ConcurrentQueue<ResultadoProcesoConcurrente> resultados =
            new ConcurrentQueue<ResultadoProcesoConcurrente>();

        private int cerrado;

        public ProcesoConcurrente Iniciar(
            string nombre,
            Func<CancellationToken, ResultadoAccion> trabajo)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException(
                    "El nombre del proceso es obligatorio.",
                    nameof(nombre));

            if (trabajo == null)
                throw new ArgumentNullException(nameof(trabajo));

            if (Volatile.Read(ref cerrado) != 0)
                throw new ObjectDisposedException(nameof(GestorProcesosConcurrentes));

            Guid procesoId = Guid.NewGuid();
            var cancelacion = new CancellationTokenSource();

            if (!cancelaciones.TryAdd(procesoId, cancelacion))
                throw new InvalidOperationException(
                    "No se pudo registrar el proceso concurrente.");

            Task finalizacion = Task.Run(() =>
            {
                int hiloTrabajo = Thread.CurrentThread.ManagedThreadId;

                try
                {
                    cancelacion.Token.ThrowIfCancellationRequested();

                    ResultadoAccion resultado =
                        trabajo(cancelacion.Token);

                    resultados.Enqueue(
                        ResultadoProcesoConcurrente.Completado(
                            procesoId,
                            nombre,
                            hiloTrabajo,
                            resultado));
                }
                catch (OperationCanceledException)
                {
                    resultados.Enqueue(
                        ResultadoProcesoConcurrente.Cancelado(
                            procesoId,
                            nombre,
                            hiloTrabajo));
                }
                catch (Exception ex)
                {
                    resultados.Enqueue(
                        ResultadoProcesoConcurrente.Fallido(
                            procesoId,
                            nombre,
                            hiloTrabajo,
                            ex.Message));
                }
                finally
                {
                    cancelaciones.TryRemove(
                        procesoId,
                        out CancellationTokenSource registrada);

                    registrada?.Dispose();
                }
            });

            return new ProcesoConcurrente(
                procesoId,
                nombre,
                finalizacion);
        }

        public bool Cancelar(Guid procesoId)
        {
            if (!cancelaciones.TryGetValue(
                procesoId,
                out CancellationTokenSource cancelacion))
            {
                return false;
            }

            cancelacion.Cancel();
            return true;
        }

        public void CancelarTodos()
        {
            foreach (CancellationTokenSource cancelacion
                     in cancelaciones.Values)
            {
                cancelacion.Cancel();
            }
        }

        public bool IntentarObtenerResultado(
            out ResultadoProcesoConcurrente resultado)
        {
            return resultados.TryDequeue(out resultado);
        }

        public int ProcesosActivos
        {
            get { return cancelaciones.Count; }
        }

        public int ResultadosPendientes
        {
            get { return resultados.Count; }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref cerrado, 1) != 0)
                return;

            CancelarTodos();
        }
    }
}
