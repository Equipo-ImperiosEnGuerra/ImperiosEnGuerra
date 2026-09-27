using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Modelo.Recursos;

namespace ImperiosEnGuerra.Api.Servicios;

/// <summary>
/// Regenera nodos físicos agotados en segundo plano.
/// El Task solo coordina tiempos y selección aleatoria; toda mutación del mapa
/// pasa por EstadoPartidaService para compartir la misma sincronización.
/// </summary>
public sealed class ServicioRegeneracionRecursos : IDisposable
{
    private readonly EstadoPartidaService estadoPartida;
    private readonly TimeSpan retardoRegeneracion;
    private readonly TimeSpan intervaloRevision;
    //Protege la lista de regeneraciones y el generador aleatorio usados por el worker.
    private readonly object sincronizacion = new();
    private readonly Dictionary<string, RegeneracionPendiente> pendientes =
        new Dictionary<string, RegeneracionPendiente>();
    private readonly Random aleatorio;

    private CancellationTokenSource? cancelacion;
    private bool dispuesto;

    //Recuerda qué nodo agotado debe reaparecer y a partir de qué momento.
    private sealed class RegeneracionPendiente
    {
        public TipoRecurso Tipo { get; }
        public Coordenada Origen { get; }
        public DateTime DisponibleUtc { get; }

        public RegeneracionPendiente(
            TipoRecurso tipo,
            Coordenada origen,
            DateTime disponibleUtc)
        {
            Tipo = tipo;
            Origen = origen;
            DisponibleUtc = disponibleUtc;
        }
    }

    public ServicioRegeneracionRecursos(
        EstadoPartidaService estadoPartida)
        : this(
            estadoPartida,
            TimeSpan.FromSeconds(12),
            TimeSpan.FromSeconds(1),
            null)
    {
    }

    public ServicioRegeneracionRecursos(
        EstadoPartidaService estadoPartida,
        TimeSpan retardoRegeneracion,
        TimeSpan intervaloRevision,
        int? semilla)
    {
        this.estadoPartida =
            estadoPartida
            ?? throw new ArgumentNullException(
                nameof(estadoPartida));

        if (retardoRegeneracion <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(retardoRegeneracion));
        }

        if (intervaloRevision <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(intervaloRevision));
        }

        this.retardoRegeneracion =
            retardoRegeneracion;

        this.intervaloRevision =
            intervaloRevision;

        aleatorio =
            semilla.HasValue
                ? new Random(
                    semilla.Value)
                : new Random();

        this.estadoPartida.PartidaFinalizada +=
            DetenerPorFinalizacion;
    }

    public bool Activo
    {
        get
        {
            lock (sincronizacion)
            {
                return cancelacion != null &&
                       !cancelacion.IsCancellationRequested;
            }
        }
    }

    public int Pendientes
    {
        get
        {
            lock (sincronizacion)
            {
                return pendientes.Count;
            }
        }
    }

    //Inicia la revisión periódica de recursos agotados en segundo plano.
    public bool Iniciar()
    {
        lock (sincronizacion)
        {
            ThrowSiDispuesto();

            if (cancelacion != null)
                return false;

            var nueva =
                new CancellationTokenSource();

            cancelacion =
                nueva;

            _ = Task.Run(
                () => EjecutarCicloAsync(
                    nueva));

            return true;
        }
    }

    public bool Detener()
    {
        lock (sincronizacion)
        {
            CancellationTokenSource? actual =
                cancelacion;

            if (actual == null)
                return false;

            try
            {
                actual.Cancel();
                pendientes.Clear();
                return true;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }
    }

    //Registra nodos agotados, espera su retardo y solicita su regeneración al estado sincronizado.
    public int EjecutarPaso()
    {
        ThrowSiDispuesto();

        IReadOnlyList<Recurso> agotados =
            estadoPartida.ObtenerRecursosAgotados();

        DateTime ahora =
            DateTime.UtcNow;

        var clavesAgotadas =
            new HashSet<string>(
                agotados.Select(
                    CrearClave));

        List<RegeneracionPendiente> vencidas;

        lock (sincronizacion)
        {
            foreach (string clave
                     in pendientes.Keys
                         .Where(
                             clave =>
                                 !clavesAgotadas.Contains(
                                     clave))
                         .ToList())
            {
                pendientes.Remove(
                    clave);
            }

            //Los nodos nuevos reciben una fecha futura antes de poder regenerarse.
            foreach (Recurso recurso
                     in agotados)
            {
                string clave =
                    CrearClave(
                        recurso);

                if (!pendientes.ContainsKey(
                        clave))
                {
                    pendientes[clave] =
                        new RegeneracionPendiente(
                            recurso.Tipo,
                            new Coordenada(
                                recurso.Coordenada.X,
                                recurso.Coordenada.Y),
                            ahora.Add(
                                retardoRegeneracion));
                }
            }

            vencidas =
                pendientes.Values
                    .Where(
                        pendiente =>
                            pendiente.DisponibleUtc <=
                            ahora)
                    .ToList();
        }

        int regeneradas = 0;

        //Solo intenta recrear los nodos cuyo tiempo de espera ya terminó.
        foreach (RegeneracionPendiente pendiente
                 in vencidas)
        {
            int selector;

            lock (sincronizacion)
            {
                selector =
                    aleatorio.Next();
            }

            bool creada =
                estadoPartida.IntentarRegenerarRecurso(
                    pendiente.Tipo,
                    pendiente.Origen,
                    selector,
                    out Coordenada nueva);

            if (!creada)
                continue;

            lock (sincronizacion)
            {
                pendientes.Remove(
                    CrearClave(
                        pendiente.Tipo,
                        pendiente.Origen));
            }

            regeneradas++;

            Console.WriteLine(
                $"RECURSO_REGENERADO: {pendiente.Tipo} -> " +
                $"({nueva.X},{nueva.Y})");
        }

        return regeneradas;
    }

    //Mantiene la revisión activa con Task.Delay cancelable para no bloquear ningún hilo.
    private async Task EjecutarCicloAsync(
        CancellationTokenSource origen)
    {
        try
        {
            while (!origen.IsCancellationRequested)
            {
                EjecutarPaso();

                await Task.Delay(
                    intervaloRevision,
                    origen.Token);
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            lock (sincronizacion)
            {
                if (ReferenceEquals(
                        cancelacion,
                        origen))
                {
                    cancelacion = null;
                }
            }

            origen.Dispose();
        }
    }

    private static string CrearClave(
        Recurso recurso)
    {
        return CrearClave(
            recurso.Tipo,
            recurso.Coordenada);
    }

    private static string CrearClave(
        TipoRecurso tipo,
        Coordenada coordenada)
    {
        return $"{tipo}:{coordenada.X}:{coordenada.Y}";
    }

    private void DetenerPorFinalizacion()
    {
        Detener();
    }

    private void ThrowSiDispuesto()
    {
        if (dispuesto)
        {
            throw new ObjectDisposedException(
                nameof(ServicioRegeneracionRecursos));
        }
    }

    //Cancela el worker, limpia pendientes y desconecta el evento de finalización.
    public void Dispose()
    {
        lock (sincronizacion)
        {
            if (dispuesto)
                return;

            dispuesto = true;

            try
            {
                cancelacion?.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }

            pendientes.Clear();
        }

        estadoPartida.PartidaFinalizada -=
            DetenerPorFinalizacion;
    }
}
