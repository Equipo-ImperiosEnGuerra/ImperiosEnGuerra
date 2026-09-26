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
    private readonly TimeSpan graciaCombateInicial;
    private readonly TimeSpan escalonCombateEntreFacciones;
    private readonly object sincronizacion = new();
    private readonly HashSet<Guid> unidadesAsignadas = new();
    private readonly HashSet<string> centrosAsignados =
        new HashSet<string>();

    // Cada facción de Máquina mantiene como máximo un frente de combate,
    // pero las tres IAs pueden combatir al mismo tiempo de forma independiente.
    private readonly HashSet<int> combatesAsignados =
        new HashSet<int>();
    private readonly HashSet<int> recoleccionesAsignadas =
        new HashSet<int>();

    private CancellationTokenSource? cancelacion;
    private DateTime inicioCicloUtc = DateTime.MinValue;
    private bool dispuesto;

    public ServicioJugadorMaquina(
        EstadoPartidaService estadoPartida,
        ServicioAccionesConcurrentes acciones)
        : this(
            estadoPartida,
            acciones,
            TimeSpan.FromMilliseconds(500),
            TimeSpan.FromSeconds(45),
            TimeSpan.FromSeconds(15))
    {
    }

    public ServicioJugadorMaquina(
        EstadoPartidaService estadoPartida,
        ServicioAccionesConcurrentes acciones,
        TimeSpan intervaloDecision)
        : this(
            estadoPartida,
            acciones,
            intervaloDecision,
            TimeSpan.Zero,
            TimeSpan.Zero)
    {
    }

    public ServicioJugadorMaquina(
        EstadoPartidaService estadoPartida,
        ServicioAccionesConcurrentes acciones,
        TimeSpan intervaloDecision,
        TimeSpan graciaCombateInicial,
        TimeSpan escalonCombateEntreFacciones)
    {
        this.estadoPartida =
            estadoPartida
            ?? throw new ArgumentNullException(nameof(estadoPartida));

        this.acciones =
            acciones
            ?? throw new ArgumentNullException(nameof(acciones));

        if (intervaloDecision <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(intervaloDecision));

        if (graciaCombateInicial < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(graciaCombateInicial));

        if (escalonCombateEntreFacciones < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(escalonCombateEntreFacciones));

        this.intervaloDecision =
            intervaloDecision;

        this.graciaCombateInicial =
            graciaCombateInicial;

        this.escalonCombateEntreFacciones =
            escalonCombateEntreFacciones;

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

            inicioCicloUtc =
                DateTime.UtcNow;

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
                return true;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }
    }

    public ProcesoConcurrente? EjecutarPaso()
    {
        ThrowSiDispuesto();

        ProcesoConcurrente? primero =
            null;

        int cantidad =
            estadoPartida.CantidadJugadoresMaquina();

        for (int indice = 0;
             indice < cantidad;
             indice++)
        {
            ProcesoConcurrente? proceso =
                EjecutarPasoMaquina(
                    indice);

            if (primero == null &&
                proceso != null)
            {
                primero =
                    proceso;
            }
        }

        return primero;
    }

    private ProcesoConcurrente? EjecutarPasoMaquina(
        int indiceMaquina)
    {
        Guid[] unidadesExcluidas;
        Coordenada[] centrosExcluidos;
        bool frenteMilitarActivo;

        lock (sincronizacion)
        {
            frenteMilitarActivo =
                combatesAsignados.Contains(
                    indiceMaquina);

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

        ProcesoConcurrente? procesoMilitar =
            null;

        if (!frenteMilitarActivo)
        {
            DecisionMaquina decisionMilitar =
                estadoPartida.PrepararDecisionMilitarMaquina(
                    indiceMaquina,
                    unidadesExcluidas,
                    CombateHabilitado(
                        indiceMaquina));

            procesoMilitar =
                EjecutarDecisionMaquina(
                    indiceMaquina,
                    decisionMilitar);
        }

        // La economía se decide de forma independiente del frente militar.
        // Así una recolección o entrenamiento no bloquea patrulla/ataque y,
        // al mismo tiempo, el combate no detiene la economía de la facción.
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

        DecisionMaquina decisionEconomica =
            estadoPartida.PrepararDecisionMaquina(
                indiceMaquina,
                unidadesExcluidas,
                centrosExcluidos,
                permitirCombate: false,
                permitirPatrulla: false);

        ProcesoConcurrente? procesoEconomico =
            EjecutarDecisionMaquina(
                indiceMaquina,
                decisionEconomica);

        return procesoMilitar ??
               procesoEconomico;
    }

    private ProcesoConcurrente? EjecutarDecisionMaquina(
        int indiceMaquina,
        DecisionMaquina decision)
    {
        if (decision == null)
            return null;

        switch (decision.Tipo)
        {
            case TipoDecisionMaquina.Recolectar:
                return EjecutarConRecoleccionAsignada(
                    indiceMaquina,
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
            case TipoDecisionMaquina.Patrullar:
                return EjecutarConCombateAsignado(
                    indiceMaquina,
                    decision.UnidadId,
                    () =>
                        acciones.IniciarMovimiento(
                            new MoverUnidadRequest
                            {
                                UnidadId =
                                    decision.UnidadId
                                        .ToString("D"),

                                Destino =
                                    new CoordenadaRequest
                                    {
                                        X =
                                            decision.Objetivo.X,

                                        Y =
                                            decision.Objetivo.Y
                                    }
                            }));

            case TipoDecisionMaquina.Atacar:
                return EjecutarConCombateAsignado(
                    indiceMaquina,
                    decision.UnidadId,
                    () =>
                        acciones.IniciarAtaque(
                            new AtacarRequest
                            {
                                AtacanteId =
                                    decision.UnidadId
                                        .ToString("D"),

                                ObjetivoId =
                                    decision.ObjetivoUnidadId
                                        .ToString("D")
                            }));

            default:
                return null;
        }
    }

    private ProcesoConcurrente? EjecutarConRecoleccionAsignada(
        int indiceMaquina,
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
            if (recoleccionesAsignadas.Contains(
                    indiceMaquina) ||
                !unidadesAsignadas.Add(
                    unidadId))
            {
                return null;
            }

            recoleccionesAsignadas.Add(
                indiceMaquina);
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

                            recoleccionesAsignadas.Remove(
                                indiceMaquina);
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

                recoleccionesAsignadas.Remove(
                    indiceMaquina);
            }

            throw;
        }
    }

    private ProcesoConcurrente? EjecutarConCombateAsignado(
        int indiceMaquina,
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
            if (combatesAsignados.Contains(
                    indiceMaquina) ||
                !unidadesAsignadas.Add(
                    unidadId))
            {
                return null;
            }

            combatesAsignados.Add(
                indiceMaquina);
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

                            combatesAsignados.Remove(
                                indiceMaquina);
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

                combatesAsignados.Remove(
                    indiceMaquina);
            }

            throw;
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
                    inicioCicloUtc =
                        DateTime.MinValue;
                }
            }

            origen.Dispose();
        }
    }

    private bool CombateHabilitado(
        int indiceMaquina)
    {
        DateTime inicio;

        lock (sincronizacion)
        {
            // Las llamadas directas usadas por pruebas y diagnóstico mantienen
            // el comportamiento inmediato. La gracia solo rige mientras el
            // ciclo real de IA está activo.
            if (cancelacion == null ||
                inicioCicloUtc ==
                    DateTime.MinValue)
            {
                return true;
            }

            inicio =
                inicioCicloUtc;
        }

        TimeSpan espera =
            graciaCombateInicial +
            TimeSpan.FromTicks(
                escalonCombateEntreFacciones.Ticks *
                Math.Max(
                    0,
                    indiceMaquina));

        return DateTime.UtcNow -
               inicio >=
               espera;
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
