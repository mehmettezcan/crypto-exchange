namespace API.Services;

public class CryptoWorker(CryptoService cryptoService, IConfiguration config) : BackgroundService
{
    private readonly CryptoService _cryptoService = cryptoService;
    private readonly IConfiguration _config = config;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var symbols = _config
            .GetSection("OkxSettings:Symbols")
            .Get<string[]>() ?? [];
            
        await _cryptoService.SubscribeToSymbolsAsync(symbols, stoppingToken);
        await _cryptoService.StartAsync(stoppingToken);
    }
}
