namespace ImperiosEnGuerra.Api.Contratos;

/// <summary>
/// Datos recibidos por la API para ordenar un ataque entre dos entidades.
/// </summary>
public sealed class AtacarRequest
{
    public string? AtacanteId { get; set; }
    public string? ObjetivoId { get; set; }
}
