namespace ImperiosEnGuerra.Api.Contratos;

public sealed class IniciarPartidaRequest
{
    public string? NombreHumano { get; set; }
    public string? NombreMaquina { get; set; }

    public int AnchoMapa { get; set; }
    public int AltoMapa { get; set; }

    public CoordenadaRequest? CentroHumano { get; set; }
    public CoordenadaRequest? CentroMaquina { get; set; }

    public List<RecursoInicialRequest>? RecursosHumano { get; set; }
    public List<RecursoInicialRequest>? RecursosMaquina { get; set; }
}

public sealed class CoordenadaRequest
{
    public int X { get; set; }
    public int Y { get; set; }
}

public sealed class RecursoInicialRequest
{
    public string? Tipo { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
}