namespace ImperiosEnGuerra.Api.Contratos;

public sealed class RecolectarRequest
{
    public string? AldeanoId { get; set; }
    public CoordenadaRequest? Objetivo { get; set; }
}