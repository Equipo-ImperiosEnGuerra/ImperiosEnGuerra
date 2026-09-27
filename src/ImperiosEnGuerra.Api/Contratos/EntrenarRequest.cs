namespace ImperiosEnGuerra.Api.Contratos;

/// <summary>
/// Datos recibidos para entrenar una unidad y definir su punto de aparición.
/// </summary>
public sealed class EntrenarRequest
{
    public CoordenadaRequest? EdificioOrigen { get; set; }
    public string? TipoUnidad { get; set; }
    public CoordenadaRequest? Destino { get; set; }
}
