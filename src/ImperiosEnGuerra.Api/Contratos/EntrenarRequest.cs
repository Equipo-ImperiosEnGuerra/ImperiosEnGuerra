namespace ImperiosEnGuerra.Api.Contratos;

public sealed class EntrenarRequest
{
    public CoordenadaRequest? EdificioOrigen { get; set; }
    public string? TipoUnidad { get; set; }
    public CoordenadaRequest? Destino { get; set; }
}
