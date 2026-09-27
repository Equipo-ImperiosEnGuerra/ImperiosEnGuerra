namespace ImperiosEnGuerra.Api.Contratos;

/// <summary>
/// Datos recibidos para mover una unidad hacia una coordenada del mapa.
/// </summary>
public sealed class MoverUnidadRequest
{
    public string? UnidadId { get; set; }
    public CoordenadaRequest? Destino { get; set; }
}
