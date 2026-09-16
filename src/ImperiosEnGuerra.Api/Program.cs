using ImperiosEnGuerra.Modelo.Map;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

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

app.Run();