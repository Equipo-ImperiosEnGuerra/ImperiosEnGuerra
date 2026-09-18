namespace ImperiosEnGuerra.Api.Contratos;

public sealed class ConstruirRequest
{
    public string? AldeanoId { get; set; }
    public string? TipoEdificio { get; set; }
    public CoordenadaRequest? Destino { get; set; }
}