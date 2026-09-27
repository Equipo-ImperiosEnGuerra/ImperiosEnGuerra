namespace ImperiosEnGuerra.Api.Contratos;

/// <summary>
/// Datos recibidos para enviar un Aldeano hacia un recurso del mapa.
/// </summary>
public sealed class RecolectarRequest
{
    public string? AldeanoId { get; set; }
    public CoordenadaRequest? Objetivo { get; set; }
}