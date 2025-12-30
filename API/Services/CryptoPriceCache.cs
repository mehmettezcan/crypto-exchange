using System.Collections.Concurrent;
using API.Controllers.DTOs;

namespace API.Services;

public class CryptoPriceCache
{
    private readonly ConcurrentDictionary<string, CryptoPriceDto> _prices = new();

    public void Update(CryptoPriceDto dto)
    {
        _prices[dto.Symbol] = dto;
    }

    public IReadOnlyCollection<CryptoPriceDto> GetAll()
    {
        return [.. _prices.Values.OrderBy(x => x.Symbol)];
    }

    public CryptoPriceDto? Get(string symbol)
    {
        _prices.TryGetValue(symbol, out var value);
        return value;
    }
}
