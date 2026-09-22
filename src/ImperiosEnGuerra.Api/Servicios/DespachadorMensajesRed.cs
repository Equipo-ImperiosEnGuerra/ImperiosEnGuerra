using System.Text.Json;
using ImperiosEnGuerra.Api.Contratos;
using ImperiosEnGuerra.Servicios.Concurrencia;

namespace ImperiosEnGuerra.Api.Servicios;

/// <summary>
/// Convierte mensajes JSON recibidos por red en las mismas acciones
/// concurrentes usadas por la API y Unity.
/// </summary>
public sealed class DespachadorMensajesRed
{
    private readonly ServicioAccionesConcurrentes acciones;

    private static readonly JsonSerializerOptions OpcionesJson =
        new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

    public DespachadorMensajesRed(
        ServicioAccionesConcurrentes acciones)
    {
        this.acciones =
            acciones
            ?? throw new ArgumentNullException(nameof(acciones));
    }

    public ResultadoDespachoRed Procesar(
        string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return ResultadoDespachoRed.Rechazado(
                string.Empty,
                "El mensaje de red está vacío.");
        }

        MensajeRedPartida? mensaje;

        try
        {
            mensaje =
                JsonSerializer.Deserialize<MensajeRedPartida>(
                    json,
                    OpcionesJson);
        }
        catch (JsonException ex)
        {
            return ResultadoDespachoRed.Rechazado(
                string.Empty,
                $"JSON inválido: {ex.Message}");
        }

        if (mensaje == null ||
            string.IsNullOrWhiteSpace(mensaje.Tipo))
        {
            return ResultadoDespachoRed.Rechazado(
                string.Empty,
                "El tipo de mensaje es obligatorio.");
        }

        string tipo =
            mensaje.Tipo
                .Trim()
                .ToUpperInvariant();

        if (mensaje.Datos.ValueKind is
            JsonValueKind.Undefined or
            JsonValueKind.Null)
        {
            return ResultadoDespachoRed.Rechazado(
                tipo,
                "El mensaje no contiene datos de acción.",
                mensaje.EmisorId,
                mensaje.MensajeId);
        }

        try
        {
            return tipo switch
            {
                "MOVER" =>
                    Iniciar(
                        mensaje,
                        Deserializar<MoverUnidadRequest>(
                            mensaje.Datos),
                        acciones.IniciarMovimiento),

                "RECOLECTAR" =>
                    Iniciar(
                        mensaje,
                        Deserializar<RecolectarRequest>(
                            mensaje.Datos),
                        acciones.IniciarRecoleccion),

                "CONSTRUIR" =>
                    Iniciar(
                        mensaje,
                        Deserializar<ConstruirRequest>(
                            mensaje.Datos),
                        acciones.IniciarConstruccion),

                "ENTRENAR" =>
                    Iniciar(
                        mensaje,
                        Deserializar<EntrenarRequest>(
                            mensaje.Datos),
                        acciones.IniciarEntrenamiento),

                "ATACAR" =>
                    Iniciar(
                        mensaje,
                        Deserializar<AtacarRequest>(
                            mensaje.Datos),
                        acciones.IniciarAtaque),

                _ =>
                    ResultadoDespachoRed.Rechazado(
                        tipo,
                        $"Tipo de mensaje no soportado: {tipo}.",
                        mensaje.EmisorId,
                        mensaje.MensajeId)
            };
        }
        catch (JsonException ex)
        {
            return ResultadoDespachoRed.Rechazado(
                tipo,
                $"Datos de acción inválidos: {ex.Message}",
                mensaje.EmisorId,
                mensaje.MensajeId);
        }
        catch (ArgumentException ex)
        {
            return ResultadoDespachoRed.Rechazado(
                tipo,
                ex.Message,
                mensaje.EmisorId,
                mensaje.MensajeId);
        }
    }

    private static T? Deserializar<T>(
        JsonElement datos)
        where T : class
    {
        return JsonSerializer.Deserialize<T>(
            datos.GetRawText(),
            OpcionesJson);
    }

    private static ResultadoDespachoRed Iniciar<T>(
        MensajeRedPartida mensaje,
        T? request,
        Func<T?, ProcesoConcurrente> iniciar)
        where T : class
    {
        if (request == null)
        {
            return ResultadoDespachoRed.Rechazado(
                mensaje.Tipo ?? string.Empty,
                "No fue posible deserializar los datos de la acción.",
                mensaje.EmisorId,
                mensaje.MensajeId);
        }

        ProcesoConcurrente proceso =
            iniciar(request);

        return ResultadoDespachoRed.Aceptado(
            mensaje,
            proceso.Id);
    }
}
