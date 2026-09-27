using System.Text.Json;

namespace ImperiosEnGuerra.Api.Contratos;

/// <summary>
/// Mensaje JSON recibido por WebSocket con tipo, emisor, identificador y datos de la acción.
/// </summary>
public sealed class MensajeRedPartida
{
    public string? Tipo { get; set; }
    public string? EmisorId { get; set; }
    public string? MensajeId { get; set; }
    public JsonElement Datos { get; set; }
}

/// <summary>
/// Respuesta del despachador que indica si el mensaje de red fue aceptado o rechazado.
/// </summary>
public sealed class ResultadoDespachoRed
{
    public bool Exito { get; init; }
    public string Tipo { get; init; } = string.Empty;
    public Guid? ProcesoId { get; init; }
    public string Mensaje { get; init; } = string.Empty;
    public string? EmisorId { get; init; }
    public string? MensajeId { get; init; }

    //Conserva los identificadores originales para poder relacionar respuesta y solicitud.
    public static ResultadoDespachoRed Aceptado(
        MensajeRedPartida mensaje,
        Guid procesoId)
    {
        return new ResultadoDespachoRed
        {
            Exito = true,
            Tipo = mensaje.Tipo?.Trim().ToUpperInvariant() ?? string.Empty,
            ProcesoId = procesoId,
            Mensaje = "Acción de red aceptada y enviada a ejecución concurrente.",
            EmisorId = mensaje.EmisorId,
            MensajeId = mensaje.MensajeId
        };
    }

    public static ResultadoDespachoRed Rechazado(
        string tipo,
        string mensaje,
        string? emisorId = null,
        string? mensajeId = null)
    {
        return new ResultadoDespachoRed
        {
            Exito = false,
            Tipo = tipo ?? string.Empty,
            ProcesoId = null,
            Mensaje = mensaje ?? string.Empty,
            EmisorId = emisorId,
            MensajeId = mensajeId
        };
    }
}
