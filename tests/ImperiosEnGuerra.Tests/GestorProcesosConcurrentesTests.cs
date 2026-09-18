using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Servicios.Concurrencia;
using NUnit.Framework;

namespace ImperiosEnGuerra.Tests
{
    public class GestorProcesosConcurrentesTests
    {
        [Test]
        public async Task DosProcesos_PuedenEstarActivosAlMismoTiempo()
        {
            using var gestor = new GestorProcesosConcurrentes();
            using var ambosIniciados = new CountdownEvent(2);
            using var liberar = new ManualResetEventSlim(false);

            ProcesoConcurrente primero = gestor.Iniciar(
                "primero",
                token =>
                {
                    ambosIniciados.Signal();
                    liberar.Wait(token);
                    return ResultadoAccion.Exitoso("primero");
                });

            ProcesoConcurrente segundo = gestor.Iniciar(
                "segundo",
                token =>
                {
                    ambosIniciados.Signal();
                    liberar.Wait(token);
                    return ResultadoAccion.Exitoso("segundo");
                });

            Assert.That(
                ambosIniciados.Wait(TimeSpan.FromSeconds(5)),
                Is.True,
                "Los dos workers deben poder iniciar antes de liberar ninguno.");

            Assert.That(gestor.ProcesosActivos, Is.EqualTo(2));

            liberar.Set();

            await Task.WhenAll(
                primero.Finalizacion,
                segundo.Finalizacion);

            List<ResultadoProcesoConcurrente> resultados =
                ExtraerResultados(gestor);

            Assert.That(resultados.Count, Is.EqualTo(2));
            Assert.That(
                resultados.All(
                    r => r.Estado == EstadoProcesoConcurrente.Completado),
                Is.True);
        }

        [Test]
        public async Task Cancelar_InterrumpeProcesoConCancellationToken()
        {
            using var gestor = new GestorProcesosConcurrentes();
            using var iniciado = new ManualResetEventSlim(false);

            ProcesoConcurrente proceso = gestor.Iniciar(
                "cancelable",
                token =>
                {
                    iniciado.Set();

                    while (true)
                    {
                        token.ThrowIfCancellationRequested();
                        Thread.Sleep(5);
                    }
                });

            Assert.That(
                iniciado.Wait(TimeSpan.FromSeconds(5)),
                Is.True);

            Assert.That(
                gestor.Cancelar(proceso.Id),
                Is.True);

            await proceso.Finalizacion;

            Assert.That(
                gestor.IntentarObtenerResultado(
                    out ResultadoProcesoConcurrente resultado),
                Is.True);

            Assert.That(
                resultado.Estado,
                Is.EqualTo(EstadoProcesoConcurrente.Cancelado));

            Assert.That(resultado.ProcesoId, Is.EqualTo(proceso.Id));
        }

        [Test]
        public async Task Resultado_SePublicaEnConcurrentQueueConIdDeWorker()
        {
            using var gestor = new GestorProcesosConcurrentes();
            int hiloLlamador = Thread.CurrentThread.ManagedThreadId;

            ProcesoConcurrente proceso = gestor.Iniciar(
                "resultado",
                _ => ResultadoAccion.Exitoso("ok"));

            await proceso.Finalizacion;

            Assert.That(
                gestor.IntentarObtenerResultado(
                    out ResultadoProcesoConcurrente resultado),
                Is.True);

            Assert.That(
                resultado.Estado,
                Is.EqualTo(EstadoProcesoConcurrente.Completado));

            Assert.That(resultado.Resultado, Is.Not.Null);
            Assert.That(resultado.Resultado.Exito, Is.True);
            Assert.That(resultado.Resultado.Mensaje, Is.EqualTo("ok"));
            Assert.That(resultado.HiloTrabajoId, Is.GreaterThan(0));
            Assert.That(resultado.HiloTrabajoId, Is.Not.EqualTo(hiloLlamador));
        }

        [Test]
        public async Task ExcepcionDelWorker_SeConvierteEnResultadoFallido()
        {
            using var gestor = new GestorProcesosConcurrentes();

            ProcesoConcurrente proceso = gestor.Iniciar(
                "fallido",
                _ => throw new InvalidOperationException("fallo controlado"));

            Assert.DoesNotThrowAsync(
                async () => await proceso.Finalizacion);

            Assert.That(
                gestor.IntentarObtenerResultado(
                    out ResultadoProcesoConcurrente resultado),
                Is.True);

            Assert.That(
                resultado.Estado,
                Is.EqualTo(EstadoProcesoConcurrente.Fallido));

            Assert.That(
                resultado.ErrorTecnico,
                Does.Contain("fallo controlado"));
        }

        [Test]
        public void ServiciosConcurrencia_NoReferenciaUnityEngine()
        {
            string[] referencias = typeof(GestorProcesosConcurrentes)
                .Assembly
                .GetReferencedAssemblies()
                .Select(assembly => assembly.Name ?? string.Empty)
                .ToArray();

            Assert.That(
                referencias.Any(
                    nombre => nombre.StartsWith(
                        "UnityEngine",
                        StringComparison.OrdinalIgnoreCase)),
                Is.False);
        }

        private static List<ResultadoProcesoConcurrente> ExtraerResultados(
            GestorProcesosConcurrentes gestor)
        {
            var resultados = new List<ResultadoProcesoConcurrente>();

            while (gestor.IntentarObtenerResultado(
                out ResultadoProcesoConcurrente resultado))
            {
                resultados.Add(resultado);
            }

            return resultados;
        }
    }
}
