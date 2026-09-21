using System;
using System.Threading;
using ImperiosEnGuerra.Modelo.Unidades;
using ImperiosEnGuerra.Modelo.Acciones;
using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Servicios.Concurrencia;
using ImperiosEnGuerra.Modelo.Edificios;
using ImperiosEnGuerra.Modelo.Map;

namespace ImperiosEnGuerra.Api.Servicios;

public sealed class ServicioAccionesConcurrentes
{
    private readonly EstadoPartidaService estadoPartida;
    private readonly GestorProcesosConcurrentes gestorProcesos;
    private readonly ServicioOrdenesUnidad servicioOrdenes;

    private readonly TimeSpan retardoMovimiento;
    private readonly TimeSpan retardoRecoleccion;
    private readonly TimeSpan retardoConstruccion;
    private readonly TimeSpan retardoEntrenamiento;
    private readonly TimeSpan retardoAtaque;


    // ============================================================
    // CONSTRUCTOR USADO POR LA API MEDIANTE INYECCIÓN DE DEPENDENCIAS
    // ============================================================
    //
    // Mantiene los tiempos de demostración actuales del proyecto:
    //
    // Movimiento:      1 segundo
    // Recolección:     2 segundos
    // Construcción:    7 segundos
    // Entrenamiento:   5 segundos
    // Ataque:          1 segundo
    //
    public ServicioAccionesConcurrentes(
        EstadoPartidaService estadoPartida,
        GestorProcesosConcurrentes gestorProcesos,
        ServicioOrdenesUnidad servicioOrdenes)
        : this(
            estadoPartida,
            gestorProcesos,
            servicioOrdenes,
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(7),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(1))
    {
    }


    // ============================================================
    // CONSTRUCTOR UTILIZADO PRINCIPALMENTE POR LAS PRUEBAS
    // ============================================================
    //
    // Permite usar, por ejemplo:
    //
    // TimeSpan.Zero
    // TimeSpan.FromMilliseconds(5)
    // TimeSpan.FromMilliseconds(100)
    // TimeSpan.FromSeconds(10)
    //
    // Así las pruebas no tienen que esperar los tiempos reales
    // del prototipo.
    //
    public ServicioAccionesConcurrentes(
        EstadoPartidaService estadoPartida,
        GestorProcesosConcurrentes gestorProcesos,
        TimeSpan retardoDemostracion)
        : this(
            estadoPartida,
            gestorProcesos,
            new ServicioOrdenesUnidad(),
            retardoDemostracion,
            retardoDemostracion,
            retardoDemostracion,
            retardoDemostracion,
            retardoDemostracion)
    {
    }


    // ============================================================
    // CONSTRUCTOR COMPLETO
    // ============================================================
    //
    // Centraliza la configuración y validación de dependencias
    // y retardos.
    //
    public ServicioAccionesConcurrentes(
        EstadoPartidaService estadoPartida,
        GestorProcesosConcurrentes gestorProcesos,
        ServicioOrdenesUnidad servicioOrdenes,
        TimeSpan retardoMovimiento,
        TimeSpan retardoRecoleccion,
        TimeSpan retardoConstruccion,
        TimeSpan retardoEntrenamiento,
        TimeSpan retardoAtaque)
    {
        this.estadoPartida =
            estadoPartida
            ?? throw new ArgumentNullException(
                nameof(estadoPartida));

        this.gestorProcesos =
            gestorProcesos
            ?? throw new ArgumentNullException(
                nameof(gestorProcesos));

        this.servicioOrdenes =
            servicioOrdenes
            ?? throw new ArgumentNullException(
                nameof(servicioOrdenes));

        ValidarRetardo(
            retardoMovimiento,
            nameof(retardoMovimiento));

        ValidarRetardo(
            retardoRecoleccion,
            nameof(retardoRecoleccion));

        ValidarRetardo(
            retardoConstruccion,
            nameof(retardoConstruccion));

        ValidarRetardo(
            retardoEntrenamiento,
            nameof(retardoEntrenamiento));

        ValidarRetardo(
            retardoAtaque,
            nameof(retardoAtaque));

        this.retardoMovimiento = retardoMovimiento;
        this.retardoRecoleccion = retardoRecoleccion;
        this.retardoConstruccion = retardoConstruccion;
        this.retardoEntrenamiento = retardoEntrenamiento;
        this.retardoAtaque = retardoAtaque;
    }


    // ============================================================
    // MOVIMIENTO
    // ============================================================

    public ProcesoConcurrente IniciarMovimiento(
        MoverUnidadRequest? request)
    {
        MoverUnidadRequest? copia =
            Copiar(request);

        return gestorProcesos.Iniciar(
            "MOVER",
            token =>
            {
                Unidad? unidad = null;

                if (Guid.TryParse(
                    copia?.UnidadId,
                    out Guid unidadId))
                {
                    unidad =
                        estadoPartida.ObtenerUnidad(
                            unidadId);
                }

                try
                {
                    EsperarAntesDeAplicar(
                        token,
                        retardoMovimiento);

                    var resultado =
                        estadoPartida.MoverUnidad(
                            copia);

                    if (resultado.Exito &&
                        unidad != null)
                    {
                        servicioOrdenes.Iniciar(
                            unidad,
                            TipoAccionJuego.Mover);
                    }

                    Console.WriteLine(
                        $"MOVIMIENTO: {resultado.Mensaje}");

                    return resultado;
                }
                finally
                {
                    if (unidad != null)
                    {
                        servicioOrdenes.Completar(
                            unidad);
                    }
                }
            });
    }


    // ============================================================
    // RECOLECCIÓN
    // ============================================================

    public ProcesoConcurrente IniciarRecoleccion(
        RecolectarRequest? request)
    {
        RecolectarRequest? copia =
            Copiar(request);

        return gestorProcesos.Iniciar(
            "RECOLECTAR",
            token =>
            {
                Unidad? unidad = null;

                if (Guid.TryParse(
                    copia?.AldeanoId,
                    out Guid unidadId))
                {
                    unidad =
                        estadoPartida.ObtenerUnidad(
                            unidadId);
                }

                try
                {
                    EsperarAntesDeAplicar(
                        token,
                        retardoRecoleccion);

                    var resultado =
                        estadoPartida.IniciarRecoleccion(
                            copia);

                    if (resultado.Exito &&
                        unidad != null)
                    {
                        servicioOrdenes.Iniciar(
                            unidad,
                            TipoAccionJuego.Recolectar);
                    }

                    return resultado;
                }
                finally
                {
                    if (unidad != null)
                    {
                        servicioOrdenes.Completar(
                            unidad);
                    }
                }
            });
    }


    // ============================================================
    // CONSTRUCCIÓN
    // ============================================================

    public ProcesoConcurrente IniciarConstruccion(
        ConstruirRequest? request)
    {
        ConstruirRequest? copia =
            Copiar(request);

        return gestorProcesos.Iniciar(
            "CONSTRUIR",
            token =>
            {
                Unidad? unidad = null;

                if (Guid.TryParse(
                    copia?.AldeanoId,
                    out Guid unidadId))
                {
                    unidad =
                        estadoPartida.ObtenerUnidad(
                            unidadId);
                }

                try
                {
                    EsperarAntesDeAplicar(
                        token,
                        retardoConstruccion);

                    var resultado =
                        estadoPartida.Construir(
                            copia);

                    if (resultado.Exito &&
                        unidad != null)
                    {
                        servicioOrdenes.Iniciar(
                            unidad,
                            TipoAccionJuego.Construir);
                    }

                    return resultado;
                }
                finally
                {
                    if (unidad != null)
                    {
                        servicioOrdenes.Completar(
                            unidad);
                    }
                }
            });
    }


    // ============================================================
    // ENTRENAMIENTO
    // ============================================================

    public ProcesoConcurrente IniciarEntrenamiento(
        EntrenarRequest? request)
    {
        EntrenarRequest? copia =
            Copiar(request);

        return gestorProcesos.Iniciar(
            "ENTRENAR",
            token =>
            {
                CentroUrbano? centro = null;

                if (copia?.EdificioOrigen != null)
                {
                    centro =
                        estadoPartida.ObtenerCentroUrbano(
                            new Coordenada(
                                copia.EdificioOrigen.X,
                                copia.EdificioOrigen.Y));
                }

                try
                {
                    if (centro != null)
                    {
                        if (!centro.IniciarEntrenamiento(
                            copia?.TipoUnidad
                            ?? string.Empty))
                        {
                            return ResultadoAccion.Fallido(
                                "El Centro Urbano ya está entrenando.");
                        }
                    }

                    EsperarAntesDeAplicar(
                        token,
                        retardoEntrenamiento);

                    return estadoPartida.Entrenar(
                        copia);
                }
                finally
                {
                    if (centro != null)
                    {
                        centro.CompletarEntrenamiento();
                    }
                }
            });
    }


    // ============================================================
    // ATAQUE
    // ============================================================

    public ProcesoConcurrente IniciarAtaque(
        AtacarRequest? request)
    {
        AtacarRequest? copia =
            Copiar(request);

        return gestorProcesos.Iniciar(
            "ATACAR",
            token =>
            {
                EsperarAntesDeAplicar(
                    token,
                    retardoAtaque);

                return estadoPartida.Atacar(
                    copia);
            });
    }


    // ============================================================
    // CANCELACIÓN
    // ============================================================

    public bool Cancelar(
        Guid procesoId)
    {
        return gestorProcesos.Cancelar(
            procesoId);
    }


    public void CancelarTodos()
    {
        gestorProcesos.CancelarTodos();
    }


    // ============================================================
    // RESULTADOS DE LOS PROCESOS
    // ============================================================

    public bool IntentarObtenerResultado(
        Guid procesoId,
        out ResultadoProcesoConcurrente resultado)
    {
        return gestorProcesos.IntentarObtenerResultado(
            procesoId,
            out resultado);
    }


    public bool IntentarObtenerResultado(
        out ResultadoProcesoConcurrente resultado)
    {
        return gestorProcesos.IntentarObtenerResultado(
            out resultado);
    }


    public int ProcesosActivos =>
        gestorProcesos.ProcesosActivos;


    // ============================================================
    // ESPERA CANCELABLE
    // ============================================================

    private static void EsperarAntesDeAplicar(
        CancellationToken token,
        TimeSpan retardo)
    {
        token.ThrowIfCancellationRequested();

        if (retardo <= TimeSpan.Zero)
        {
            return;
        }

        if (token.WaitHandle.WaitOne(
            retardo))
        {
            token.ThrowIfCancellationRequested();
        }
    }


    // ============================================================
    // VALIDACIÓN DE RETARDOS
    // ============================================================

    private static void ValidarRetardo(
        TimeSpan retardo,
        string nombreParametro)
    {
        if (retardo < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nombreParametro);
        }
    }


    // ============================================================
    // COPIAS DE REQUESTS
    // ============================================================
    //
    // Los workers trabajan con copias de los datos recibidos.
    // Esto evita depender de objetos que podrían ser modificados
    // externamente mientras una Task se está ejecutando.
    // ============================================================

    private static EntrenarRequest? Copiar(
        EntrenarRequest? request)
    {
        if (request == null)
        {
            return null;
        }

        return new EntrenarRequest
        {
            TipoUnidad =
                request.TipoUnidad,

            EdificioOrigen =
                request.EdificioOrigen == null
                    ? null
                    : new CoordenadaRequest
                    {
                        X =
                            request.EdificioOrigen.X,

                        Y =
                            request.EdificioOrigen.Y
                    },

            Destino =
                request.Destino == null
                    ? null
                    : new CoordenadaRequest
                    {
                        X =
                            request.Destino.X,

                        Y =
                            request.Destino.Y
                    }
        };
    }


    private static ConstruirRequest? Copiar(
        ConstruirRequest? request)
    {
        if (request == null)
        {
            return null;
        }

        return new ConstruirRequest
        {
            AldeanoId =
                request.AldeanoId,

            TipoEdificio =
                request.TipoEdificio,

            Destino =
                request.Destino == null
                    ? null
                    : new CoordenadaRequest
                    {
                        X =
                            request.Destino.X,

                        Y =
                            request.Destino.Y
                    }
        };
    }


    private static RecolectarRequest? Copiar(
        RecolectarRequest? request)
    {
        if (request == null)
        {
            return null;
        }

        return new RecolectarRequest
        {
            AldeanoId =
                request.AldeanoId,

            Objetivo =
                request.Objetivo == null
                    ? null
                    : new CoordenadaRequest
                    {
                        X =
                            request.Objetivo.X,

                        Y =
                            request.Objetivo.Y
                    }
        };
    }


    private static MoverUnidadRequest? Copiar(
        MoverUnidadRequest? request)
    {
        if (request == null)
        {
            return null;
        }

        return new MoverUnidadRequest
        {
            UnidadId =
                request.UnidadId,

            Destino =
                request.Destino == null
                    ? null
                    : new CoordenadaRequest
                    {
                        X =
                            request.Destino.X,

                        Y =
                            request.Destino.Y
                    }
        };
    }


    private static AtacarRequest? Copiar(
        AtacarRequest? request)
    {
        if (request == null)
        {
            return null;
        }

        return new AtacarRequest
        {
            AtacanteId =
                request.AtacanteId,

            ObjetivoId =
                request.ObjetivoId
        };
    }
}