using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using ImperiosEnGuerra.Api.Contratos;

namespace ImperiosEnGuerra.Api.Servicios;

/// <summary>
/// Mantiene varias conexiones WebSocket y escucha cada instancia de forma
/// concurrente. Los mensajes válidos se despachan a gameplay y la respuesta
/// se difunde a todos los clientes conectados.
/// </summary>
public sealed class ServicioRedPartida
{
    private const int TamanoBuffer = 4096;
    private const int TamanoMaximoMensaje = 65536;

    //Mantiene las conexiones activas sin bloquear cuando varios clientes entran o salen a la vez.
    private readonly ConcurrentDictionary<Guid, ConexionRed>
        conexiones = new();

    private readonly DespachadorMensajesRed despachador;

    private static readonly JsonSerializerOptions OpcionesJson =
        new(JsonSerializerDefaults.Web);

    public ServicioRedPartida(
        DespachadorMensajesRed despachador)
    {
        this.despachador =
            despachador
            ?? throw new ArgumentNullException(nameof(despachador));
    }

    public int ClientesConectados =>
        conexiones.Count;

    //Escucha un cliente hasta que se desconecta o se cancela la petición.
    public async Task AtenderClienteAsync(
        WebSocket socket,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(socket);

        Guid id =
            Guid.NewGuid();

        var conexion =
            new ConexionRed(
                id,
                socket);

        if (!conexiones.TryAdd(
                id,
                conexion))
        {
            throw new InvalidOperationException(
                "No se pudo registrar la conexión WebSocket.");
        }

        try
        {
            //Cada conexión mantiene su propia espera asíncrona sin bloquear a las demás.
            while (!cancellationToken.IsCancellationRequested &&
                   socket.State == WebSocketState.Open)
            {
                string? json =
                    await RecibirTextoAsync(
                        socket,
                        cancellationToken);

                if (json == null)
                    break;

                ResultadoDespachoRed resultado =
                    despachador.Procesar(
                        json);

                await DifundirAsync(
                    resultado,
                    cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (WebSocketException)
        {
        }
        finally
        {
            conexiones.TryRemove(
                id,
                out _);

            await CerrarSeguroAsync(
                socket);

            conexion.Dispose();
        }
    }

    //Une los fragmentos de WebSocket hasta reconstruir un mensaje de texto completo.
    private async Task<string?> RecibirTextoAsync(
        WebSocket socket,
        CancellationToken cancellationToken)
    {
        byte[] buffer =
            new byte[TamanoBuffer];

        using var memoria =
            new MemoryStream();

        while (true)
        {
            WebSocketReceiveResult resultado =
                await socket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    cancellationToken);

            if (resultado.MessageType ==
                WebSocketMessageType.Close)
            {
                return null;
            }

            if (resultado.MessageType !=
                WebSocketMessageType.Text)
            {
                return string.Empty;
            }

            memoria.Write(
                buffer,
                0,
                resultado.Count);

            //Limita el tamaño acumulado para evitar mensajes excesivos en memoria.
            if (memoria.Length >
                TamanoMaximoMensaje)
            {
                throw new WebSocketException(
                    "El mensaje WebSocket excede el tamaño permitido.");
            }

            if (resultado.EndOfMessage)
                break;
        }

        return Encoding.UTF8.GetString(
            memoria.ToArray());
    }

    //Serializa una sola respuesta y la envía a todos los clientes conectados.
    private async Task DifundirAsync(
        ResultadoDespachoRed resultado,
        CancellationToken cancellationToken)
    {
        string json =
            JsonSerializer.Serialize(
                resultado,
                OpcionesJson);

        byte[] bytes =
            Encoding.UTF8.GetBytes(
                json);

        ConexionRed[] actuales =
            conexiones.Values.ToArray();

        Task[] envios =
            actuales
                .Select(
                    conexion =>
                        EnviarSeguroAsync(
                            conexion,
                            bytes,
                            cancellationToken))
                .ToArray();

        if (envios.Length > 0)
        {
            //Espera todos los envíos en paralelo sin enviar cliente por cliente.
            await Task.WhenAll(
                envios);
        }
    }

    //El semáforo evita que dos respuestas escriban al mismo WebSocket al mismo tiempo.
    private async Task EnviarSeguroAsync(
        ConexionRed conexion,
        byte[] bytes,
        CancellationToken cancellationToken)
    {
        await conexion.BloqueoEnvio.WaitAsync(
            cancellationToken);

        try
        {
            if (conexion.Socket.State !=
                WebSocketState.Open)
            {
                return;
            }

            await conexion.Socket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
        catch (WebSocketException)
        {
        }
        finally
        {
            conexion.BloqueoEnvio.Release();
        }
    }

    private static async Task CerrarSeguroAsync(
        WebSocket socket)
    {
        try
        {
            if (socket.State is
                WebSocketState.Open or
                WebSocketState.CloseReceived)
            {
                await socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Conexión finalizada.",
                    CancellationToken.None);
            }
        }
        catch (WebSocketException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
    }

    //Agrupa el socket y su bloqueo de envío para cada cliente conectado.
    private sealed class ConexionRed : IDisposable
    {
        public Guid Id { get; }
        public WebSocket Socket { get; }
        public SemaphoreSlim BloqueoEnvio { get; } =
            new(1, 1);

        public ConexionRed(
            Guid id,
            WebSocket socket)
        {
            Id = id;
            Socket = socket;
        }

        public void Dispose()
        {
            BloqueoEnvio.Dispose();
        }
    }
}
