namespace API.Interfaces;

public interface ICryptoService
{
    Task SubscribeToSymbolsAsync(IEnumerable<string> symbols, CancellationToken ct = default);
    Task StartAsync(CancellationToken ct = default);
    Task StopAsync();
    bool IsConnected { get; }
}

