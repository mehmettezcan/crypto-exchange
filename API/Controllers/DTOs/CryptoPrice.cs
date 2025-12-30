namespace API.Controllers.DTOs;

public class CryptoPriceDto
{
    public string Symbol { get; set; } = string.Empty;
    public string LastPrice { get; set; } = string.Empty;
    public string High24h { get; set; } = string.Empty;
    public string Low24h { get; set; } = string.Empty;
    public string Volume24h { get; set; } = string.Empty;
    public string Change24h { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

