using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using API.Controllers.DTOs;
using API.Interfaces;

namespace API.Services;

public class CryptoService(IConfiguration configuration) : ICryptoService
{
    private readonly IConfiguration _configuration = configuration;

    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _cts;

    private readonly HashSet<string> _subscribedSymbols = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lockObject = new();

    private Task? _receiveLoopTask;
    private Task? _keepAliveTask;

    private int _reconnectInProgress = 0;

    public event EventHandler<CryptoPriceDto>? PriceUpdated;

    public bool IsConnected => _webSocket?.State == WebSocketState.Open;

    public async Task SubscribeToSymbolsAsync(IEnumerable<string> symbols, CancellationToken ct = default)
    {
        List<string> newlyAdded = [];

        lock (_lockObject)
        {
            foreach (var s in symbols)
            {
                if (_subscribedSymbols.Add(s))
                    newlyAdded.Add(s);
            }
        }

        if (newlyAdded.Count == 0) return;

        if (IsConnected)
        {
            await SubscribeToTickersAsync(newlyAdded, ct);
        }
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        if (IsConnected) return;

        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _webSocket = new ClientWebSocket();

        var wsUrl = _configuration["OkxSettings:WebSocketUrl"]
                    ?? "wss://ws.okx.com:8443/ws/v5/public";

        await _webSocket.ConnectAsync(new Uri(wsUrl), _cts.Token);

        string[] snapshot;
        lock (_lockObject)
        {
            snapshot = _subscribedSymbols.ToArray();
        }

        if (snapshot.Length > 0)
        {
            await SubscribeToTickersAsync(snapshot, _cts.Token);
        }

        _receiveLoopTask = Task.Run(() => ReceiveLoopAsync(_cts.Token), _cts.Token);
        _keepAliveTask = Task.Run(() => KeepAliveLoopAsync(_cts.Token), _cts.Token);
    }

    public async Task StopAsync()
    {
        try
        {
            _cts?.Cancel();
        }
        catch { /* ignore */ }

        if (_webSocket != null)
        {
            try
            {
                if (_webSocket.State == WebSocketState.Open)
                {
                    await _webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Closing",
                        CancellationToken.None);
                }
            }
            catch { /* ignore */ }

            _webSocket.Dispose();
        }

        _webSocket = null;
        _cts?.Dispose();
        _cts = null;

        _receiveLoopTask = null;
        _keepAliveTask = null;
    }

    private async Task SubscribeToTickersAsync(IEnumerable<string> symbols, CancellationToken ct)
    {
        if (!IsConnected || _webSocket == null) return;

        var msg = new
        {
            op = "subscribe",
            args = symbols.Select(s => new { channel = "tickers", instId = s }).ToArray()
        };

        var json = JsonSerializer.Serialize(msg);
        var bytes = Encoding.UTF8.GetBytes(json);

        await _webSocket.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            endOfMessage: true,
            cancellationToken: ct);
    }

    private async Task KeepAliveLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(20), ct);

                if (!IsConnected || _webSocket == null) continue;

                var pingBytes = Encoding.UTF8.GetBytes("ping");
                await _webSocket.SendAsync(
                    new ArraySegment<byte>(pingBytes),
                    WebSocketMessageType.Text,
                    endOfMessage: true,
                    cancellationToken: ct);
            }
            catch (OperationCanceledException) { }
            catch
            {
                // Receive loop zaten reconnect tetikleyecek.
            }
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        var buffer = new byte[1024 * 16];
        var sb = new StringBuilder();

        while (!ct.IsCancellationRequested)
        {
            try
            {
                if (_webSocket == null || _webSocket.State != WebSocketState.Open)
                {
                    await EnsureReconnectAsync(ct);
                    continue;
                }

                sb.Clear();

                WebSocketReceiveResult result;
                do
                {
                    result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await EnsureReconnectAsync(ct);
                        goto ContinueOuter;
                    }

                    var chunk = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    sb.Append(chunk);

                } while (!result.EndOfMessage);

                var message = sb.ToString();

                if (message == "pong" || message == "ping")
                    goto ContinueOuter;

                ProcessMessage(message);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket receive error: {ex.Message}");
                await EnsureReconnectAsync(ct);
            }

        ContinueOuter:
            continue;
        }
    }

    private void ProcessMessage(string message)
    {
        try
        {
            using var doc = JsonDocument.Parse(message);
            var root = doc.RootElement;

            if (!root.TryGetProperty("data", out var dataArray) || dataArray.ValueKind != JsonValueKind.Array)
                return;

            foreach (var item in dataArray.EnumerateArray())
            {
                var symbol = item.TryGetProperty("instId", out var instId) ? instId.GetString() ?? "" : "";
                if (string.IsNullOrWhiteSpace(symbol)) continue;

                var last = item.TryGetProperty("last", out var lastEl) ? lastEl.GetString() ?? "0" : "0";
                var high24h = item.TryGetProperty("high24h", out var highEl) ? highEl.GetString() ?? "0" : "0";
                var low24h = item.TryGetProperty("low24h", out var lowEl) ? lowEl.GetString() ?? "0" : "0";
                var vol24h = item.TryGetProperty("vol24h", out var volEl) ? volEl.GetString() ?? "0" : "0";
                var change24h = item.TryGetProperty("change24h", out var chEl) ? chEl.GetString() ?? "0" : "0";

                var dto = new CryptoPriceDto
                {
                    Symbol = symbol,
                    LastPrice = last,
                    High24h = high24h,
                    Low24h = low24h,
                    Volume24h = vol24h,
                    Change24h = change24h,
                    Timestamp = DateTime.UtcNow
                };

                PriceUpdated?.Invoke(this, dto);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing message: {ex.Message}");
        }
    }

    private async Task EnsureReconnectAsync(CancellationToken ct)
    {
        if (Interlocked.Exchange(ref _reconnectInProgress, 1) == 1)
            return;

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(2), ct);

            if (_webSocket != null)
            {
                try
                {
                    if (_webSocket.State == WebSocketState.Open || _webSocket.State == WebSocketState.CloseReceived)
                    {
                        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Reconnecting", CancellationToken.None);
                    }
                }
                catch { /* ignore */ }

                _webSocket.Dispose();
            }

            _webSocket = new ClientWebSocket();

            var wsUrl = _configuration["OkxSettings:WebSocketUrl"]
                        ?? "wss://ws.okx.com:8443/ws/v5/public";

            await _webSocket.ConnectAsync(new Uri(wsUrl), ct);

            string[] snapshot;
            lock (_lockObject)
            {
                snapshot = _subscribedSymbols.ToArray();
            }

            if (snapshot.Length > 0)
            {
                await SubscribeToTickersAsync(snapshot, ct);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Console.WriteLine($"Reconnect failed: {ex.Message}");
        }
        finally
        {
            Interlocked.Exchange(ref _reconnectInProgress, 0);
        }
    }
}
