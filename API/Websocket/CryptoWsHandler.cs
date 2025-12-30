using System.Net.WebSockets;
using System.Text.Json;
using API.Services;

namespace API.Websocket;

public class CryptoWsHandler(CryptoWsConnectionManager manager, CryptoPriceCache cache)
{
    private readonly CryptoWsConnectionManager _manager = manager;
    private readonly CryptoPriceCache _cache = cache;

    public async Task HandleAsync(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        using var ws = await context.WebSockets.AcceptWebSocketAsync();
        var id = _manager.Add(ws);

        var snapshot = _cache.GetAll();
        var snapshotJson = JsonSerializer.Serialize(new
        {
            type = "snapshot",
            data = snapshot
        });
        await _manager.BroadcastAsync(snapshotJson);

        var buffer = new byte[8 * 1024];
        try
        {
            while (ws.State == WebSocketState.Open)
            {
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close) break;
            }
        }
        finally
        {
            await _manager.RemoveAsync(id);
        }
    }
}
