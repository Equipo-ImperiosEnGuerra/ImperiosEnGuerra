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
    private readonly ServicioRegeneracionRecursos regeneracionRecursos;
    private readonly TimeSpan tiempoMaximoSinLatido;
    private readonly TimeSpan intervaloRevision;
    private readonly object sincronizacion = new();

    private readonly CancellationTokenSource cancelacionMonitor =
        new CancellationTokenSource();

    private readonly Task tareaMonitor;

    private DateTime ultimoLatidoUtc;
    private bool activa;
    private bool pausaTemporal;
    private bool dispuesto;

    public ServicioSesionJuego(
        EstadoPartidaService estadoPartida,
        ServicioAccionesConcurrentes acciones,
        ServicioJugadorMaquina jugadorMaquina,
        ServicioReaccionesAutomaticas reacciones,
        ServicioRegeneracionRecursos regeneracionRecursos)
        : this(
            estadoPartida,
            acciones,
            jugadorMaquina,
            reacciones,
            regeneracionRecursos,
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(1))
    {
    }

    public ServicioSesionJuego(
        EstadoPartidaService estadoPartida,
        ServicioAccionesConcurrentes acciones,
        ServicioJugadorMaquina jugadorMaquina,
        ServicioReaccionesAutomaticas reacciones,
        ServicioRegeneracionRecursos regeneracionRecursos,
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

        this.regeneracionRecursos =
            regeneracionRecursos
            ?? throw new ArgumentNullException(
                nameof(regeneracionRecursos));

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

    public bool PausadaTemporalmente
    {
        get
        {
            lock (sincronizacion)
            {
                return pausaTemporal;
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
            pausaTemporal = false;
            activa = true;
            ultimoLatidoUtc =
                DateTime.UtcNow;
        }

        acciones.ReanudarTemporal();
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
            ultimoLatidoUtc =
                DateTime.UtcNow;

            // Un GET de snapshot no debe quitar una pausa solicitada
            // explícitamente desde el menú.
            if (pausaTemporal)
            {
                return true;
            }

            activa = true;
        }

        AsegurarServiciosActivos();
        return true;
    }

    public bool PausarTemporal()
    {
        ThrowSiDispuesto();

        if (!estadoPartida.HayPartidaActiva() ||
            estadoPartida.EstaFinalizada())
        {
            return false;
        }

        bool cambio;

        lock (sincronizacion)
        {
            cambio =
                !pausaTemporal;

            pausaTemporal =
                true;

            activa =
                false;
        }

        acciones.PausarTemporal();
        jugadorMaquina.Detener();
        reacciones.Detener();
        regeneracionRecursos.Detener();

        return cambio;
    }

    public bool ReanudarTemporal()
    {
        ThrowSiDispuesto();

        if (!estadoPartida.HayPartidaActiva() ||
            estadoPartida.EstaFinalizada())
        {
            return false;
        }

        bool estabaPausada;

        lock (sincronizacion)
        {
            estabaPausada =
                pausaTemporal;

            pausaTemporal =
                false;

            activa =
                true;

            ultimoLatidoUtc =
                DateTime.UtcNow;
        }

        acciones.ReanudarTemporal();
        AsegurarServiciosActivos();

        return estabaPausada;
    }

    public bool Pausar()
    {
        bool estabaActiva;

        lock (sincronizacion)
        {
            estabaActiva =
                activa ||
                pausaTemporal;

            activa =
                false;

            pausaTemporal =
                false;
        }

        jugadorMaquina.Detener();
        reacciones.Detener();
        regeneracionRecursos.Detener();

        // La pausa dura conserva workers; esta pausa de sesión, usada al
        // salir/expirar, sí debe cancelarlos y dejar la puerta lista para una
        // futura partida.
        acciones.ReanudarTemporal();
        acciones.CancelarTodos();

        return estabaActiva;
    }

    private void AsegurarServiciosActivos()
    {
        jugadorMaquina.Iniciar();
        reacciones.Iniciar();
        regeneracionRecursos.Iniciar();
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

                bool debePausar = false;

                lock (sincronizacion)
                {
                    if (activa &&
                        ultimoLatidoUtc !=
                            DateTime.MinValue &&
                        DateTime.UtcNow -
                            ultimoLatidoUtc >
                            tiempoMaximoSinLatido)
                    {
                        activa = false;
                        debePausar = true;
                    }
                }

                if (debePausar)
                {
                    jugadorMaquina.Detener();
                    reacciones.Detener();
                    regeneracionRecursos.Detener();
                    acciones.CancelarTodos();
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
