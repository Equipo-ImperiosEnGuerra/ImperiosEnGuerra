namespace ImperiosEnGuerra.Api.Contratos;

/// <summary>
/// Configuración recibida para crear el mapa, participantes y recursos iniciales.
/// </summary>
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

/// <summary>
/// Coordenada simple usada por los contratos HTTP antes de convertirla al Modelo.
/// </summary>
public sealed class CoordenadaRequest
{
    public int X { get; set; }
    public int Y { get; set; }
}

/// <summary>
/// Recurso físico que se colocará al crear la partida.
/// </summary>
public sealed class RecursoInicialRequest
{
    public string? Tipo { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
}