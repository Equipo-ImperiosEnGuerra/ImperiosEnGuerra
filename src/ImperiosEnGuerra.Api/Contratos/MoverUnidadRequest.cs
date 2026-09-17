namespace ImperiosEnGuerra.Api.Contratos;

public sealed class MoverUnidadRequest
{
    public string? UnidadId { get; set; }
    public CoordenadaRequest? Destino { get; set; }
}
