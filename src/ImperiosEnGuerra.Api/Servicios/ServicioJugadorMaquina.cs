using System.Collections.Generic;
using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Modelo.IA;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Servicios.Concurrencia;

namespace ImperiosEnGuerra.Api.Servicios;

public sealed class ServicioJugadorMaquina : IDisposable
{
    private readonly EstadoPartidaService estadoPartida;
    private readonly ServicioAccionesConcurrentes acciones;
    private readonly TimeSpan intervaloDecision;
    private readonly object sincronizacion = new();
    private readonly HashSet<Guid> unidadesAsignadas = new();
    private readonly HashSet<string> centrosAsignados =
        new HashSet<string>();

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

    public ProcesoConcurrente? EjecutarPaso()
    {
        ThrowSiDispuesto();

        Guid[] unidadesExcluidas;
        Coordenada[] centrosExcluidos;

        lock (sincronizacion)
        {
            unidadesExcluidas =
                unidadesAsignadas.ToArray();

            centrosExcluidos =
                centrosAsignados
                    .Select(
                        ParsearCentro)
                    .Where(
                        c => c != null)
                    .ToArray();
        }

        DecisionMaquina decision =
            estadoPartida.PrepararDecisionMaquina(
                unidadesExcluidas,
                centrosExcluidos);

        switch (decision.Tipo)
        {
            case TipoDecisionMaquina.Recolectar:
                return EjecutarConUnidadAsignada(
                    decision.UnidadId,
                    () =>
                        acciones.IniciarRecoleccion(
                            new RecolectarRequest
                            {
                                AldeanoId =
                                    decision.UnidadId
                                        .ToString("D"),

                                Objetivo =
                                    new CoordenadaRequest
                                    {
                                        X =
                                            decision.Objetivo.X,

                                        Y =
                                            decision.Objetivo.Y
                                    }
                            }));

            case TipoDecisionMaquina.Construir:
                return EjecutarConUnidadAsignada(
                    decision.UnidadId,
                    () =>
                        acciones.IniciarConstruccion(
                            new ConstruirRequest
                            {
                                AldeanoId =
                                    decision.UnidadId
                                        .ToString("D"),

                                TipoEdificio =
                                    decision.TipoEdificio,

                                Destino =
                                    new CoordenadaRequest
                                    {
                                        X =
                                            decision.Objetivo.X,

                                        Y =
                                            decision.Objetivo.Y
                                    }
                            }));

            case TipoDecisionMaquina.Entrenar:
                return EjecutarConCentroAsignado(
                    decision.EdificioOrigen,
                    () =>
                        acciones.IniciarEntrenamiento(
                            new EntrenarRequest
                            {
                                EdificioOrigen =
                                    new CoordenadaRequest
                                    {
                                        X =
                                            decision.EdificioOrigen.X,

                                        Y =
                                            decision.EdificioOrigen.Y
                                    },

                                TipoUnidad =
                                    decision.TipoUnidad
                            }));

            case TipoDecisionMaquina.Mover:
                return EjecutarConUnidadAsignada(
                    decision.UnidadId,
                    () =>
                        acciones.IniciarMovimiento(
                            new MoverUnidadRequest
                            {
                                UnidadId =
                                    decision.UnidadId.ToString("D"),
                                Destino =
                                    new CoordenadaRequest
                                    {
                                        X = decision.Objetivo.X,
                                        Y = decision.Objetivo.Y
                                    }
                            }));

            case TipoDecisionMaquina.Atacar:
                return EjecutarConUnidadAsignada(
                    decision.UnidadId,
                    () =>
                        acciones.IniciarAtaque(
                            new AtacarRequest
                            {
                                AtacanteId =
                                    decision.UnidadId.ToString("D"),
                                ObjetivoId =
                                    decision.ObjetivoUnidadId.ToString("D")
                            }));

            default:
                return null;
        }
    }

    private ProcesoConcurrente? EjecutarConUnidadAsignada(
        Guid unidadId,
        Func<ProcesoConcurrente> iniciar)
    {
        if (unidadId == Guid.Empty ||
            iniciar == null)
        {
            return null;
        }

        lock (sincronizacion)
        {
            if (!unidadesAsignadas.Add(
                    unidadId))
            {
                return null;
            }
        }

        try
        {
            ProcesoConcurrente proceso =
                iniciar();

            _ = proceso.Finalizacion
                .ContinueWith(
                    _ =>
                    {
                        lock (sincronizacion)
                        {
                            unidadesAsignadas.Remove(
                                unidadId);
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
                    unidadId);
            }

            throw;
        }
    }

    private ProcesoConcurrente? EjecutarConCentroAsignado(
        Coordenada centro,
        Func<ProcesoConcurrente> iniciar)
    {
        if (centro == null ||
            iniciar == null)
        {
            return null;
        }

        string clave =
            ClaveCentro(
                centro);

        lock (sincronizacion)
        {
            if (!centrosAsignados.Add(
                    clave))
            {
                return null;
            }
        }

        try
        {
            ProcesoConcurrente proceso =
                iniciar();

            _ = proceso.Finalizacion
                .ContinueWith(
                    _ =>
                    {
                        lock (sincronizacion)
                        {
                            centrosAsignados.Remove(
                                clave);
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
                centrosAsignados.Remove(
                    clave);
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

    private static string ClaveCentro(
        Coordenada centro)
    {
        return $"{centro.X}:{centro.Y}";
    }

    private static Coordenada? ParsearCentro(
        string clave)
    {
        string[] partes =
            clave.Split(':');

        if (partes.Length != 2 ||
            !int.TryParse(
                partes[0],
                out int x) ||
            !int.TryParse(
                partes[1],
                out int y))
        {
            return null;
        }

        return new Coordenada(
            x,
            y);
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
                nameof(ServicioJugadorMaquina));
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
