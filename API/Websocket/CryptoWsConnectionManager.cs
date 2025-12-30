using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace API.Websocket;

public class CryptoWsConnectionManager
{
    private readonly ConcurrentDictionary<string, WebSocket> _clients = new();

    public string Add(WebSocket ws)
    {
        var id = Guid.NewGuid().ToString("N");
        _clients.TryAdd(id, ws);
        return id;
    }

    public async Task RemoveAsync(string id)
    {
        if (_clients.TryRemove(id, out var ws))
        {
            try
            {
                if (ws.State == WebSocketState.Open || ws.State == WebSocketState.CloseReceived)
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
            }
            catch { /* ignore */ }
        }
    }

    public async Task BroadcastAsync(string message, CancellationToken ct = default)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        var segment = new ArraySegment<byte>(bytes);

        foreach (var kv in _clients)
        {
            var ws = kv.Value;

            if (ws.State != WebSocketState.Open)
            {
                await RemoveAsync(kv.Key);
                continue;
            }

            try
            {
                await ws.SendAsync(segment, WebSocketMessageType.Text, true, ct);
            }
            catch
            {
                await RemoveAsync(kv.Key);
            }
        }
    }
}
