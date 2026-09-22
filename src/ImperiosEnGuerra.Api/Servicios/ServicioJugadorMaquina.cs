using System.Collections.Generic;
using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Modelo.IA;
using ImperiosEnGuerra.Servicios.Concurrencia;

namespace ImperiosEnGuerra.Api.Servicios;

public sealed class ServicioJugadorMaquina : IDisposable
{
    private readonly EstadoPartidaService estadoPartida;
    private readonly ServicioAccionesConcurrentes acciones;
    private readonly TimeSpan intervaloDecision;
    private readonly object sincronizacion = new();
    private readonly HashSet<Guid> unidadesAsignadas = new();

    private CancellationTokenSource? cancelacion;
    private bool dispuesto;

    public ServicioJugadorMaquina(
        EstadoPartidaService estadoPartida,
        ServicioAccionesConcurrentes acciones)
        : this(
            estadoPartida,
            acciones,
            TimeSpan.FromMilliseconds(500))
    {
    }

    public ServicioJugadorMaquina(
        EstadoPartidaService estadoPartida,
        ServicioAccionesConcurrentes acciones,
        TimeSpan intervaloDecision)
    {
        this.estadoPartida =
            estadoPartida
            ?? throw new ArgumentNullException(nameof(estadoPartida));

        this.acciones =
            acciones
            ?? throw new ArgumentNullException(nameof(acciones));

        if (intervaloDecision <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(intervaloDecision));

        this.intervaloDecision =
            intervaloDecision;
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

    public int UnidadesAsignadas
    {
        get
        {
            lock (sincronizacion)
            {
                return unidadesAsignadas.Count;
            }
        }
    }

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
                () => EjecutarCicloAsync(nueva));

            return true;
        }
    }

    public bool Detener()
    {
        CancellationTokenSource? actual;

        lock (sincronizacion)
        {
            actual = cancelacion;
        }

        if (actual == null)
            return false;

        actual.Cancel();
        return true;
    }

    public ProcesoConcurrente? EjecutarPaso()
    {
        ThrowSiDispuesto();

        Guid[] excluidas;

        lock (sincronizacion)
        {
            excluidas =
                unidadesAsignadas.ToArray();
        }

        DecisionMaquina decision =
            estadoPartida.PrepararDecisionMaquina(
                excluidas);

        if (decision.Tipo != TipoDecisionMaquina.Recolectar ||
            decision.Objetivo == null)
        {
            return null;
        }

        lock (sincronizacion)
        {
            if (!unidadesAsignadas.Add(
                    decision.UnidadId))
            {
                return null;
            }
        }

        try
        {
            ProcesoConcurrente proceso =
                acciones.IniciarRecoleccion(
                    new RecolectarRequest
                    {
                        AldeanoId =
                            decision.UnidadId.ToString("D"),

                        Objetivo =
                            new CoordenadaRequest
                            {
                                X = decision.Objetivo.X,
                                Y = decision.Objetivo.Y
                            }
                    });

            _ = proceso.Finalizacion.ContinueWith(
                _ =>
                {
                    lock (sincronizacion)
                    {
                        unidadesAsignadas.Remove(
                            decision.UnidadId);
                    }
                },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);

            return proceso;
        }
        catch
        {
            lock (sincronizacion)
            {
                unidadesAsignadas.Remove(
                    decision.UnidadId);
            }

            throw;
        }
    }

    private async Task EjecutarCicloAsync(
        CancellationTokenSource origen)
    {
        try
        {
            while (!origen.IsCancellationRequested)
            {
                EjecutarPaso();

                await Task.Delay(
                    intervaloDecision,
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

    private void ThrowSiDispuesto()
    {
        if (dispuesto)
            throw new ObjectDisposedException(
                nameof(ServicioJugadorMaquina));
    }

    public void Dispose()
    {
        CancellationTokenSource? actual;

        lock (sincronizacion)
        {
            if (dispuesto)
                return;

            dispuesto = true;
            actual = cancelacion;
        }

        actual?.Cancel();
    }
}
