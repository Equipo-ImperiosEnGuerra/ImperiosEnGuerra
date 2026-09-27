namespace ImperiosEnGuerra.Api.Contratos;

/// <summary>
/// Datos recibidos para indicar qué Aldeano construye, qué edificio y en qué posición.
/// </summary>
public sealed class ConstruirRequest
{
    public string? AldeanoId { get; set; }
    public string? TipoEdificio { get; set; }
    public CoordenadaRequest? Destino { get; set; }
}