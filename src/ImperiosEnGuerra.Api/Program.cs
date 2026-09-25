using System.Net.WebSockets;
using Microsoft.Extensions.Options;
using ImperiosEnGuerra.Api.Configuracion;
using ImperiosEnGuerra.Api.Servicios;
using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Api.Mapeadores;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;
using ImperiosEnGuerra.Servicios;
using ImperiosEnGuerra.Servicios.Concurrencia;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton(
    new ServicioArchivos(
        Path.Combine(
            Directory.GetCurrentDirectory(),
            "DatosPartida")));
builder.Services.AddSingleton<EstadoPartidaService>();
builder.Services.AddSingleton<GestorProcesosConcurrentes>();
builder.Services.AddSingleton<ServicioOrdenesUnidad>();
builder.Services.AddSingleton<ServicioAccionesConcurrentes>();
builder.Services.AddSingleton<ServicioReaccionesAutomaticas>();
builder.Services.AddSingleton<ServicioJugadorMaquina>();
builder.Services.AddSingleton<DespachadorMensajesRed>();
builder.Services.AddSingleton<ServicioRedPartida>();


var app = builder.Build();

app.UseWebSockets();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/api/estado", () =>
{
    return Results.Ok(new
    {
        estado = "activo",
        servicio = "ImperiosEnGuerra.Api"
    });
})
.WithName("ObtenerEstado");

app.MapGet("/api/modelo/prueba", () =>
{
    Mapa mapa = new Mapa(10, 8);

    return Results.Ok(new
    {
        modelo = "conectado",
        ancho = mapa.Ancho,
        alto = mapa.Alto,
        casillas = mapa.Ancho * mapa.Alto
    });
})
.WithName("ProbarModelo");

app.MapPost(
    "/api/partida/iniciar",
    (
        IniciarPartidaRequest request,
        EstadoPartidaService estadoPartida,
        ServicioAccionesConcurrentes accionesConcurrentes,
        ServicioReaccionesAutomaticas reaccionesAutomaticas,
        ServicioJugadorMaquina jugadorMaquina,
        ServicioArchivos servicioArchivos) =>
{
    try
    {
        jugadorMaquina.Detener();
        reaccionesAutomaticas.Detener();
        accionesConcurrentes.CancelarTodos();

        Mapa mapa = new Mapa(
            request.AnchoMapa,
            request.AltoMapa);

        Coordenada centroHumano =
            PartidaRequestMapper.ConvertirCoordenada(
                request.CentroHumano);

        Coordenada centroMaquina =
            PartidaRequestMapper.ConvertirCoordenada(
                request.CentroMaquina);

        var recursosHumano =
            PartidaRequestMapper.ConvertirRecursos(
                request.RecursosHumano);

        var recursosMaquina =
            PartidaRequestMapper.ConvertirRecursos(
                request.RecursosMaquina);

        var inicializador = new InicializadorPartida();

        Partida partida = inicializador.Crear(
            request.NombreHumano ?? string.Empty,
            mapa,
            centroHumano,
            recursosHumano,
            request.NombreMaquina ?? string.Empty,
            mapa,
            centroMaquina,
            recursosMaquina);

        servicioArchivos.GuardarConfiguracionInicial(
            partida);

        estadoPartida.EstablecerPartida(partida);
        reaccionesAutomaticas.Iniciar();
        jugadorMaquina.Iniciar();

        return Results.Ok(new
        {
            estado = "iniciada",

            mapa = new
            {
                ancho = mapa.Ancho,
                alto = mapa.Alto
            },

            jugadorHumano = new
            {
                nombre = partida.JugadorHumano.Nombre,
                tipo = partida.JugadorHumano.Tipo.ToString(),
                edificios = partida.JugadorHumano.Edificios.Count,
                unidades = partida.JugadorHumano.Unidades.Count
            },

            jugadorMaquina = new
            {
                nombre = partida.JugadorMaquina.Nombre,
                tipo = partida.JugadorMaquina.Tipo.ToString(),
                edificios = partida.JugadorMaquina.Edificios.Count,
                unidades = partida.JugadorMaquina.Unidades.Count
            }
        });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new
        {
            error = ex.Message
        });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new
        {
            error = ex.Message
        });
    }
    catch (IOException ex)
    {
        return Results.Problem(
            title: "No se pudo guardar configuracion.txt.",
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
    }
    catch (UnauthorizedAccessException ex)
    {
        return Results.Problem(
            title: "No se pudo guardar configuracion.txt.",
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
    }
})
.WithName("IniciarPartida");

app.MapGet("/api/partida", (EstadoPartidaService estadoPartida) =>
{
    EstadoPartidaResponse? respuesta = estadoPartida.ObtenerEstado();

    if (respuesta == null)
    {
        return Results.NotFound(new
        {
            error = "No hay una partida activa."
        });
    }

    return Results.Ok(respuesta);
})
.WithName("ObtenerPartidaActiva");

app.MapPost(
    "/api/partida/mover",
    (MoverUnidadRequest? request, EstadoPartidaService estadoPartida) =>
{
    var resultado = estadoPartida.MoverUnidad(request);

    return resultado.Exito
        ? Results.Ok(resultado)
        : Results.BadRequest(resultado);
})
.WithName("MoverUnidad");

app.MapPost(
    "/api/partida/mover-concurrente",
    (
        MoverUnidadRequest? request,
        ServicioAccionesConcurrentes accionesConcurrentes) =>
{
    var proceso =
        accionesConcurrentes.IniciarMovimiento(request);

    return Results.Accepted(
        $"/api/procesos/{proceso.Id}",
        new
        {
            procesoId = proceso.Id,
            nombre = proceso.Nombre,
            estado = "iniciado"
        });
})
.WithName("IniciarMovimientoConcurrente");

app.MapGet(
    "/api/procesos/{procesoId:guid}/resultado",
    (
        Guid procesoId,
        ServicioAccionesConcurrentes accionesConcurrentes) =>
{
    if (!accionesConcurrentes.IntentarObtenerResultado(
        procesoId,
        out ResultadoProcesoConcurrente resultado))
    {
        return Results.NoContent();
    }

    return Results.Ok(new
    {
        procesoId = resultado.ProcesoId,
        nombre = resultado.Nombre,
        estado = resultado.Estado.ToString(),
        hiloTrabajoId = resultado.HiloTrabajoId,
        exito = resultado.Resultado?.Exito ?? false,
        mensaje = resultado.Resultado?.Mensaje,
        errorTecnico = resultado.ErrorTecnico
    });
})
.WithName("ObtenerResultadoProcesoPorId");

app.MapGet(
    "/api/procesos/resultado",
    (ServicioAccionesConcurrentes accionesConcurrentes) =>
{
    if (!accionesConcurrentes.IntentarObtenerResultado(
        out ResultadoProcesoConcurrente resultado))
    {
        return Results.NoContent();
    }

    return Results.Ok(new
    {
        procesoId = resultado.ProcesoId,
        nombre = resultado.Nombre,
        estado = resultado.Estado.ToString(),
        hiloTrabajoId = resultado.HiloTrabajoId,
        exito = resultado.Resultado?.Exito ?? false,
        mensaje = resultado.Resultado?.Mensaje,
        errorTecnico = resultado.ErrorTecnico
    });
})
.WithName("ObtenerResultadoProceso");

app.MapPost(
    "/api/procesos/{procesoId:guid}/cancelar",
    (
        Guid procesoId,
        ServicioAccionesConcurrentes accionesConcurrentes) =>
{
    return accionesConcurrentes.Cancelar(procesoId)
        ? Results.Ok(new
        {
            procesoId,
            estado = "cancelacion_solicitada"
        })
        : Results.NotFound(new
        {
            error = "No existe un proceso activo con ese ID."
        });
})
.WithName("CancelarProceso");

app.MapPost(
    "/api/partida/recolectar-concurrente",
    (
        RecolectarRequest? request,
        ServicioAccionesConcurrentes accionesConcurrentes) =>
{
    var proceso =
        accionesConcurrentes.IniciarRecoleccion(request);

    return Results.Accepted(
        $"/api/procesos/{proceso.Id}",
        new
        {
            procesoId = proceso.Id,
            nombre = proceso.Nombre,
            estado = "iniciado"
        });
})
.WithName("IniciarRecoleccionConcurrente");

app.MapPost(
    "/api/partida/recolectar",
    (RecolectarRequest? request, EstadoPartidaService estadoPartida) =>
{
    var resultado = estadoPartida.IniciarRecoleccion(request);

    return resultado.Exito
        ? Results.Ok(resultado)
        : Results.BadRequest(resultado);
})
.WithName("IniciarRecoleccion");

app.MapPost(
    "/api/partida/construir-concurrente",
    (
        ConstruirRequest? request,
        ServicioAccionesConcurrentes accionesConcurrentes) =>
{
    var proceso =
        accionesConcurrentes.IniciarConstruccion(request);

    return Results.Accepted(
        $"/api/procesos/{proceso.Id}",
        new
        {
            procesoId = proceso.Id,
            nombre = proceso.Nombre,
            estado = "iniciado"
        });
})
.WithName("IniciarConstruccionConcurrente");

app.MapPost(
    "/api/partida/construir",
    (ConstruirRequest? request, EstadoPartidaService estadoPartida) =>
{
    var resultado = estadoPartida.Construir(request);

    return resultado.Exito
        ? Results.Ok(resultado)
        : Results.BadRequest(resultado);
})
.WithName("Construir");


app.MapPost(
    "/api/partida/entrenar-concurrente",
    (
        EntrenarRequest? request,
        ServicioAccionesConcurrentes accionesConcurrentes) =>
{
    var proceso =
        accionesConcurrentes.IniciarEntrenamiento(request);

    return Results.Accepted(
        $"/api/procesos/{proceso.Id}",
        new
        {
            procesoId = proceso.Id,
            nombre = proceso.Nombre,
            estado = "iniciado"
        });
})
.WithName("IniciarEntrenamientoConcurrente");

app.MapPost(
    "/api/partida/entrenar",
    (EntrenarRequest? request, EstadoPartidaService estadoPartida) =>
{
    var resultado = estadoPartida.Entrenar(request);

    return resultado.Exito
        ? Results.Ok(resultado)
        : Results.BadRequest(resultado);
})
.WithName("Entrenar");

app.MapPost(
    "/api/partida/atacar-concurrente",
    (
        AtacarRequest? request,
        ServicioAccionesConcurrentes accionesConcurrentes) =>
{
    ProcesoConcurrente proceso =
        accionesConcurrentes.IniciarAtaque(request);

    return Results.Accepted(
        $"/api/procesos/{proceso.Id}",
        new
        {
            procesoId = proceso.Id,
            nombre = proceso.Nombre,
            estado = "iniciado"
        });
})
.WithName("IniciarAtaqueConcurrente");


app.MapPost(
    "/api/partida/curar-concurrente",
    (
        CurarRequest? request,
        ServicioAccionesConcurrentes accionesConcurrentes) =>
{
    ProcesoConcurrente proceso =
        accionesConcurrentes.IniciarCuracion(request);

    return Results.Accepted(
        $"/api/procesos/{proceso.Id}",
        new
        {
            procesoId = proceso.Id,
            nombre = proceso.Nombre,
            estado = "iniciado"
        });
})
.WithName("IniciarCuracionConcurrente");


app.MapGet(
    "/api/reacciones/estado",
    (ServicioReaccionesAutomaticas reacciones) =>
{
    return Results.Ok(new
    {
        activo = reacciones.Activo,
        unidadesAsignadas =
            reacciones.UnidadesAsignadas,
        radioDeteccionMilitar =
            ImperiosEnGuerra.Modelo.Combate
                .PlanificadorReaccionAutomatica
                .RadioDeteccionMilitarPredeterminado
    });
})
.WithName("ObtenerEstadoReaccionesAutomaticas");


app.MapGet(
    "/api/maquina/estado",
    (ServicioJugadorMaquina jugadorMaquina) =>
{
    return Results.Ok(new
    {
        activo = jugadorMaquina.Activo,
        unidadesAsignadas = jugadorMaquina.UnidadesAsignadas
    });
})
.WithName("ObtenerEstadoJugadorMaquina");

app.MapPost(
    "/api/maquina/iniciar",
    (ServicioJugadorMaquina jugadorMaquina) =>
{
    return Results.Ok(new
    {
        iniciado = jugadorMaquina.Iniciar(),
        activo = jugadorMaquina.Activo
    });
})
.WithName("IniciarJugadorMaquina");

app.MapPost(
    "/api/maquina/detener",
    (ServicioJugadorMaquina jugadorMaquina) =>
{
    return Results.Ok(new
    {
        cancelacionSolicitada = jugadorMaquina.Detener()
    });
})
.WithName("DetenerJugadorMaquina");

app.MapPost(
    "/api/maquina/paso",
    (ServicioJugadorMaquina jugadorMaquina) =>
{
    ProcesoConcurrente? proceso =
        jugadorMaquina.EjecutarPaso();

    return proceso == null
        ? Results.Ok(new
        {
            estado = "sin_accion"
        })
        : Results.Accepted(
            $"/api/procesos/{proceso.Id}",
            new
            {
                procesoId = proceso.Id,
                nombre = proceso.Nombre,
                estado = "iniciado"
            });
})
.WithName("EjecutarPasoJugadorMaquina");


app.MapGet(
    "/api/red/estado",
    (ServicioRedPartida red) =>
{
    return Results.Ok(new
    {
        transporte = "WebSocket",
        endpoint = "/ws/partida",
        clientesConectados = red.ClientesConectados
    });
})
.WithName("ObtenerEstadoRed");

app.Map(
    "/ws/partida",
    async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode =
            StatusCodes.Status400BadRequest;

        await context.Response.WriteAsJsonAsync(
            new
            {
                error =
                    "Este endpoint requiere una conexión WebSocket."
            });

        return;
    }

    ServicioRedPartida red =
        context.RequestServices
            .GetRequiredService<ServicioRedPartida>();

    using WebSocket socket =
        await context.WebSockets
            .AcceptWebSocketAsync();

    await red.AtenderClienteAsync(
        socket,
        context.RequestAborted);
});

app.Run();