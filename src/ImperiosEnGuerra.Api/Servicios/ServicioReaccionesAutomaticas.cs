using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Modelo.Combate;
using ImperiosEnGuerra.Servicios.Concurrencia;

namespace ImperiosEnGuerra.Api.Servicios;

/// <summary>
/// Ejecuta reacciones automáticas de unidades humanas sin reemplazar las órdenes manuales del jugador.
/// </summary>
public sealed class ServicioReaccionesAutomaticas : IDisposable
{
    private readonly EstadoPartidaService estadoPartida;
    private readonly ServicioAccionesConcurrentes acciones;
    private readonly TimeSpan intervaloDeteccion;
    //Protege asignaciones y suspensiones porque el detector y los workers trabajan en paralelo.
    private readonly object sincronizacion = new();
    private readonly HashSet<Guid> unidadesAsignadas = new();
    private readonly HashSet<Guid> unidadesSuspendidas =
        new HashSet<Guid>();
    private readonly Dictionary<Guid, ProcesoConcurrente> procesosAsignados =
        new Dictionary<Guid, ProcesoConcurrente>();
    private readonly Dictionary<Guid, DateTime> proximoMovimientoIdle =
        new Dictionary<Guid, DateTime>();
    private readonly HashSet<Guid> movimientosIdleActivos =
        new HashSet<Guid>();
    private static readonly TimeSpan IntervaloMovimientoIdle =
        TimeSpan.FromSeconds(8);

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

    //Inicia el detector periódico en una Task independiente.
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

    //Cancela una reacción automática activa antes de entregar la unidad a una orden manual.
    public void PrepararOrdenManual(
        Guid unidadId)
    {
        if (unidadId == Guid.Empty)
            return;

        ProcesoConcurrente proceso = null;

        lock (sincronizacion)
        {
            unidadesSuspendidas.Remove(
                unidadId);

            procesosAsignados.TryGetValue(
                unidadId,
                out proceso);
        }

        if (proceso == null)
            return;

        acciones.Cancelar(
            proceso.Id);

        try
        {
            proceso.Finalizacion.Wait(
                TimeSpan.FromMilliseconds(500));
        }
        catch (AggregateException)
        {
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
                return true;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }
    }

    //Consulta las reacciones posibles y evita asignar dos procesos automáticos a la misma unidad.
    public int EjecutarPaso()
    {
        ThrowSiDispuesto();

        IReadOnlyList<ReaccionAutomatica> reacciones =
            estadoPartida.PrepararReaccionesAutomaticas();

        int iniciadas = 0;

        bool paseoIdleOcupado =
            HayMovimientoIdleActivo();

        foreach (ReaccionAutomatica reaccion in reacciones)
        {
            if (EstaSuspendida(
                    reaccion.UnidadId) ||
                EnEnfriamientoIdle(
                    reaccion))
            {
                continue;
            }

            if (reaccion.Tipo ==
                    TipoReaccionAutomatica.MoverIdle &&
                paseoIdleOcupado)
            {
                continue;
            }

            if (EjecutarReaccion(
                    reaccion) != null)
            {
                iniciadas++;

                if (reaccion.Tipo ==
                    TipoReaccionAutomatica.MoverIdle)
                {
                    paseoIdleOcupado =
                        true;
                }
            }
        }

        return iniciadas;
    }

    private bool HayMovimientoIdleActivo()
    {
        lock (sincronizacion)
        {
            return movimientosIdleActivos.Count >
                   0;
        }
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

    private bool EnEnfriamientoIdle(
        ReaccionAutomatica reaccion)
    {
        if (reaccion == null ||
            reaccion.Tipo !=
                TipoReaccionAutomatica.MoverIdle)
        {
            return false;
        }

        lock (sincronizacion)
        {
            if (!proximoMovimientoIdle.TryGetValue(
                    reaccion.UnidadId,
                    out DateTime proximo))
            {
                return false;
            }

            return DateTime.UtcNow <
                   proximo;
        }
    }

    //Convierte la reacción preparada en un ataque, curación o movimiento idle concurrente.
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
            ProcesoConcurrente proceso;

            if (reaccion.Tipo ==
                TipoReaccionAutomatica.Curar)
            {
                proceso =
                    acciones.IniciarCuracion(
                        new CurarRequest
                        {
                            CuradorId =
                                reaccion.UnidadId
                                    .ToString("D"),

                            ObjetivoId =
                                reaccion.ObjetivoId
                                    .ToString("D")
                        });
            }
            else if (reaccion.Tipo ==
                     TipoReaccionAutomatica.MoverIdle)
            {
                proceso =
                    acciones.IniciarMovimientoIdle(
                        new MoverUnidadRequest
                        {
                            UnidadId =
                                reaccion.UnidadId
                                    .ToString("D"),

                            Destino =
                                new CoordenadaRequest
                                {
                                    X =
                                        reaccion.Destino.X,

                                    Y =
                                        reaccion.Destino.Y
                                }
                        });
            }
            else
            {
                proceso =
                    acciones.IniciarAtaque(
                        new AtacarRequest
                        {
                            AtacanteId =
                                reaccion.UnidadId
                                    .ToString("D"),

                            ObjetivoId =
                                reaccion.ObjetivoId
                                    .ToString("D")
                        });
            }

            lock (sincronizacion)
            {
                procesosAsignados[
                    reaccion.UnidadId] =
                    proceso;

                if (reaccion.Tipo ==
                    TipoReaccionAutomatica.MoverIdle)
                {
                    movimientosIdleActivos.Add(
                        reaccion.UnidadId);
                }
            }

            //Libera la unidad cuando termina el worker para que pueda recibir otra reacción.
            _ = proceso.Finalizacion
                .ContinueWith(
                    _ =>
                    {
                        lock (sincronizacion)
                        {
                            unidadesAsignadas.Remove(
                                reaccion.UnidadId);

                            procesosAsignados.Remove(
                                reaccion.UnidadId);

                            if (reaccion.Tipo ==
                                TipoReaccionAutomatica.MoverIdle)
                            {
                                movimientosIdleActivos.Remove(
                                    reaccion.UnidadId);

                                proximoMovimientoIdle[
                                    reaccion.UnidadId] =
                                    DateTime.UtcNow.Add(
                                        IntervaloMovimientoIdle);
                            }
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

                procesosAsignados.Remove(
                    reaccion.UnidadId);

                movimientosIdleActivos.Remove(
                    reaccion.UnidadId);
            }

            throw;
        }
    }

    //Revisa periódicamente el estado mientras el servicio no haya sido cancelado.
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

    //Cancela el detector y deja de escuchar el evento de finalización de partida.
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
                // El ciclo pudo finalizar y disponer su token justo antes.
                // Dispose debe seguir siendo idempotente y seguro.
            }
        }

        estadoPartida.PartidaFinalizada -=
            DetenerPorFinalizacion;
    }
}
