namespace ImperiosEnGuerra.Api.Contratos;

public sealed class EstadoPartidaResponse
{
    public required string Estado { get; init; }
    public required MapaEstadoResponse Mapa { get; init; }
    public required JugadorEstadoResponse JugadorHumano { get; init; }
    public required JugadorEstadoResponse JugadorMaquina { get; init; }
    public required EconomiaEstadoResponse Economia { get; init; }
}

public sealed class EconomiaEstadoResponse
{
    public required CostoEstadoResponse CentroUrbano { get; init; }
    public required CostoEstadoResponse Aldeano { get; init; }
    public required CostoEstadoResponse Guerrero { get; init; }
    public required CostoEstadoResponse Lancero { get; init; }
    public required CostoEstadoResponse Arquero { get; init; }
    public required CostoEstadoResponse Monje { get; init; }
}

public sealed class CostoEstadoResponse
{
    public int Oro { get; init; }
    public int Madera { get; init; }
    public int Comida { get; init; }
}

public sealed class MapaEstadoResponse
{
    public int Ancho { get; init; }
    public int Alto { get; init; }
    public required IReadOnlyList<RecursoEstadoResponse> Recursos { get; init; }
}

public sealed class JugadorEstadoResponse
{
    public required string Nombre { get; init; }
    public required string Tipo { get; init; }
    public required RecursosJugadorEstadoResponse Recursos { get; init; }
    public required IReadOnlyList<EdificioEstadoResponse> Edificios { get; init; }
    public required IReadOnlyList<ObraConstruccionEstadoResponse> ObrasConstruccion { get; init; }
    public required IReadOnlyList<UnidadEstadoResponse> Unidades { get; init; }
}

public sealed class RecursosJugadorEstadoResponse
{
    public int Oro { get; init; }
    public int Madera { get; init; }
    public int Comida { get; init; }
}

public sealed class RecursoEstadoResponse
{
    public required string Tipo { get; init; }
    public required CoordenadaEstadoResponse Coordenada { get; init; }
    public int CantidadRestante { get; init; }
}

public sealed class EdificioEstadoResponse
{
    public required string Id { get; init; }
    public required string Tipo { get; init; }
    public required CoordenadaEstadoResponse Coordenada { get; init; }
    public required IReadOnlyList<EntrenamientoEstadoResponse> ColaEntrenamiento { get; init; }
}

public sealed class EntrenamientoEstadoResponse
{
    public required string Id { get; init; }
    public required string TipoUnidad { get; init; }
    public int Progreso { get; init; }
}

public sealed class ObraConstruccionEstadoResponse
{
    public required string Id { get; init; }
    public required string Tipo { get; init; }
    public required CoordenadaEstadoResponse Coordenada { get; init; }
    public int Progreso { get; init; }
}

public sealed class UnidadEstadoResponse
{
    public required string Id { get; init; }
    public required string Tipo { get; init; }
    public CoordenadaEstadoResponse? Coordenada { get; init; }
    public bool Disponible { get; init; }

    public required string Estado { get; init; }
    public string? OrdenActiva { get; init; }

    public int CapacidadCarga { get; init; }
    public int CargaActual { get; init; }
    public string? TipoCarga { get; init; }
}

public sealed class CoordenadaEstadoResponse
{
    public int X { get; init; }
    public int Y { get; init; }
}
