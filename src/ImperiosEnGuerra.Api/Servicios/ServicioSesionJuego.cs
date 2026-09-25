using System;
using System.Threading;
using System.Threading.Tasks;

namespace ImperiosEnGuerra.Api.Servicios;

/// <summary>
/// Coordina el ciclo de vida de la simulación respecto al cliente Unity.
/// Si dejan de llegar snapshots/latidos, detiene IA, reacciones y workers
/// para evitar que la partida continúe avanzando fuera de sesión.
/// </summary>
public sealed class ServicioSesionJuego : IDisposable
{
    private readonly EstadoPartidaService estadoPartida;
    private readonly ServicioAccionesConcurrentes acciones;
    private readonly ServicioJugadorMaquina jugadorMaquina;
    private readonly ServicioReaccionesAutomaticas reacciones;
    private readonly TimeSpan tiempoMaximoSinLatido;
    private readonly TimeSpan intervaloRevision;
    private readonly object sincronizacion = new();

    private readonly CancellationTokenSource cancelacionMonitor =
        new CancellationTokenSource();

    private readonly Task tareaMonitor;

    private DateTime ultimoLatidoUtc;
    private bool activa;
    private bool dispuesto;

    public ServicioSesionJuego(
        EstadoPartidaService estadoPartida,
        ServicioAccionesConcurrentes acciones,
        ServicioJugadorMaquina jugadorMaquina,
        ServicioReaccionesAutomaticas reacciones)
        : this(
            estadoPartida,
            acciones,
            jugadorMaquina,
            reacciones,
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(1))
    {
    }

    public ServicioSesionJuego(
        EstadoPartidaService estadoPartida,
        ServicioAccionesConcurrentes acciones,
        ServicioJugadorMaquina jugadorMaquina,
        ServicioReaccionesAutomaticas reacciones,
        TimeSpan tiempoMaximoSinLatido,
        TimeSpan intervaloRevision)
    {
        this.estadoPartida =
            estadoPartida
            ?? throw new ArgumentNullException(
                nameof(estadoPartida));

        this.acciones =
            acciones
            ?? throw new ArgumentNullException(
                nameof(acciones));

        this.jugadorMaquina =
            jugadorMaquina
            ?? throw new ArgumentNullException(
                nameof(jugadorMaquina));

        this.reacciones =
            reacciones
            ?? throw new ArgumentNullException(
                nameof(reacciones));

        if (tiempoMaximoSinLatido <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tiempoMaximoSinLatido));
        }

        if (intervaloRevision <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(intervaloRevision));
        }

        this.tiempoMaximoSinLatido =
            tiempoMaximoSinLatido;

        this.intervaloRevision =
            intervaloRevision;

        ultimoLatidoUtc =
            DateTime.MinValue;

        tareaMonitor =
            Task.Run(
                () => MonitorearAsync(
                    cancelacionMonitor.Token));
    }

    public bool Activa
    {
        get
        {
            lock (sincronizacion)
            {
                return activa;
            }
        }
    }

    public DateTime UltimoLatidoUtc
    {
        get
        {
            lock (sincronizacion)
            {
                return ultimoLatidoUtc;
            }
        }
    }

    public bool Activar()
    {
        ThrowSiDispuesto();

        if (!estadoPartida.HayPartidaActiva() ||
            estadoPartida.EstaFinalizada())
        {
            return false;
        }

        lock (sincronizacion)
        {
            activa = true;
            ultimoLatidoUtc =
                DateTime.UtcNow;
        }

        AsegurarServiciosActivos();
        return true;
    }

    public bool RegistrarLatido()
    {
        ThrowSiDispuesto();

        if (!estadoPartida.HayPartidaActiva() ||
            estadoPartida.EstaFinalizada())
        {
            Pausar();
            return false;
        }

        lock (sincronizacion)
        {
            activa = true;
            ultimoLatidoUtc =
                DateTime.UtcNow;
        }

        // Si el monitor había detenido recientemente los ciclos,
        // los siguientes latidos los reactivan cuando su cancelación
        // ya terminó de propagarse.
        AsegurarServiciosActivos();
        return true;
    }

    public bool Pausar()
    {
        bool estabaActiva;

        lock (sincronizacion)
        {
            estabaActiva =
                activa;

            activa =
                false;
        }

        jugadorMaquina.Detener();
        reacciones.Detener();
        acciones.CancelarTodos();

        return estabaActiva;
    }

    private void AsegurarServiciosActivos()
    {
        jugadorMaquina.Iniciar();
        reacciones.Iniciar();
    }

    private async Task MonitorearAsync(
        CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(
                    intervaloRevision,
                    token);

                bool vencida;

                lock (sincronizacion)
                {
                    vencida =
                        activa &&
                        ultimoLatidoUtc !=
                            DateTime.MinValue &&
                        DateTime.UtcNow -
                            ultimoLatidoUtc >
                            tiempoMaximoSinLatido;
                }

                if (vencida)
                {
                    Pausar();
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void ThrowSiDispuesto()
    {
        if (dispuesto)
        {
            throw new ObjectDisposedException(
                nameof(ServicioSesionJuego));
        }
    }

    public void Dispose()
    {
        if (dispuesto)
            return;

        dispuesto = true;

        Pausar();
        cancelacionMonitor.Cancel();

        try
        {
            tareaMonitor.Wait(
                TimeSpan.FromSeconds(1));
        }
        catch (AggregateException)
        {
        }

        cancelacionMonitor.Dispose();
    }
}
