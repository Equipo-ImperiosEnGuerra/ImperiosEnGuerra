using ImperiosEnGuerra.Api.Servicios;
using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Api.Mapeadores;
using ImperiosEnGuerra.Modelo.Core;
using ImperiosEnGuerra.Modelo.Map;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<EstadoPartidaService>();

var app = builder.Build();

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
    (IniciarPartidaRequest request, EstadoPartidaService estadoPartida) =>
{
    try
    {
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

        estadoPartida.EstablecerPartida(partida);

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
    "/api/partida/recolectar",
    (RecolectarRequest? request, EstadoPartidaService estadoPartida) =>
{
    var resultado = estadoPartida.IniciarRecoleccion(request);

    return resultado.Exito
        ? Results.Ok(resultado)
        : Results.BadRequest(resultado);
})
.WithName("IniciarRecoleccion");

app.Run();