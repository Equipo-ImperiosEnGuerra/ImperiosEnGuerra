using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Modelo.Combate;
using ImperiosEnGuerra.Servicios.Concurrencia;

namespace ImperiosEnGuerra.Api.Servicios;

public sealed class ServicioReaccionesAutomaticas : IDisposable
{
    private readonly EstadoPartidaService estadoPartida;
    private readonly ServicioAccionesConcurrentes acciones;
    private readonly TimeSpan intervaloDeteccion;
    private readonly object sincronizacion = new();
    private readonly HashSet<Guid> unidadesAsignadas = new();
    private readonly HashSet<Guid> unidadesSuspendidas =
        new HashSet<Guid>();

    private CancellationTokenSource? cancelacion;
    private bool dispuesto;

    public ServicioReaccionesAutomaticas(
        EstadoPartidaService estadoPartida,
        ServicioAccionesConcurrentes acciones)
        : this(
            estadoPartida,
            acciones,
            TimeSpan.FromMilliseconds(500))
    {
    }

    public ServicioReaccionesAutomaticas(
        EstadoPartidaService estadoPartida,
        ServicioAccionesConcurrentes acciones,
        TimeSpan intervaloDeteccion)
    {
        this.estadoPartida =
            estadoPartida
            ?? throw new ArgumentNullException(
                nameof(estadoPartida));

        this.acciones =
            acciones
            ?? throw new ArgumentNullException(
                nameof(acciones));

        if (intervaloDeteccion <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(intervaloDeteccion));
        }

        this.intervaloDeteccion =
            intervaloDeteccion;

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
                () => EjecutarCicloAsync(
                    nueva));

            return true;
        }
    }

    public void SuspenderUnidad(
        Guid unidadId)
    {
        if (unidadId == Guid.Empty)
            return;

        lock (sincronizacion)
        {
            unidadesSuspendidas.Add(
                unidadId);
        }
    }

    public void ReanudarUnidad(
        Guid unidadId)
    {
        if (unidadId == Guid.Empty)
            return;

        lock (sincronizacion)
        {
            unidadesSuspendidas.Remove(
                unidadId);
        }
    }

    public bool Detener()
    {
        CancellationTokenSource? actual;

        lock (sincronizacion)
        {
            actual =
                cancelacion;
        }

        if (actual == null)
            return false;

        actual.Cancel();
        return true;
    }

    public int EjecutarPaso()
    {
        ThrowSiDispuesto();

        IReadOnlyList<ReaccionAutomatica> reacciones =
            estadoPartida.PrepararReaccionesAutomaticas();

        int iniciadas = 0;

        foreach (ReaccionAutomatica reaccion in reacciones)
        {
            if (EstaSuspendida(
                    reaccion.UnidadId))
            {
                continue;
            }

            if (EjecutarReaccion(
                    reaccion) != null)
            {
                iniciadas++;
            }
        }

        return iniciadas;
    }

    private bool EstaSuspendida(
        Guid unidadId)
    {
        lock (sincronizacion)
        {
            return unidadesSuspendidas.Contains(
                unidadId);
        }
    }

    private ProcesoConcurrente? EjecutarReaccion(
        ReaccionAutomatica reaccion)
    {
        if (reaccion == null)
            return null;

        lock (sincronizacion)
        {
            if (!unidadesAsignadas.Add(
                    reaccion.UnidadId))
            {
                return null;
            }
        }

        try
        {
            ProcesoConcurrente proceso =
                reaccion.Tipo ==
                TipoReaccionAutomatica.Curar
                    ? acciones.IniciarCuracion(
                        new CurarRequest
                        {
                            CuradorId =
                                reaccion.UnidadId
                                    .ToString("D"),

                            ObjetivoId =
                                reaccion.ObjetivoId
                                    .ToString("D")
                        })
                    : acciones.IniciarAtaque(
                        new AtacarRequest
                        {
                            AtacanteId =
                                reaccion.UnidadId
                                    .ToString("D"),

                            ObjetivoId =
                                reaccion.ObjetivoId
                                    .ToString("D")
                        });

            _ = proceso.Finalizacion
                .ContinueWith(
                    _ =>
                    {
                        lock (sincronizacion)
                        {
                            unidadesAsignadas.Remove(
                                reaccion.UnidadId);
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
                    reaccion.UnidadId);
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
                    intervaloDeteccion,
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

    private void DetenerPorFinalizacion()
    {
        Detener();
    }

    private void ThrowSiDispuesto()
    {
        if (dispuesto)
        {
            throw new ObjectDisposedException(
                nameof(ServicioReaccionesAutomaticas));
        }
    }

    public void Dispose()
    {
        CancellationTokenSource? actual;

        lock (sincronizacion)
        {
            if (dispuesto)
                return;

            dispuesto = true;
            actual =
                cancelacion;
        }

        estadoPartida.PartidaFinalizada -=
            DetenerPorFinalizacion;

        actual?.Cancel();
    }
}
