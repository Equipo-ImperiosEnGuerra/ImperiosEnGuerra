namespace ImperiosEnGuerra.Api.Contratos;

/// <summary>
/// Datos recibidos para identificar al Monje y a la unidad aliada que debe curar.
/// </summary>
public sealed class CurarRequest
{
    public string? CuradorId { get; set; }
    public string? ObjetivoId { get; set; }
}
