using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CryptoController(CryptoPriceCache priceCache) : ControllerBase
{
    private readonly CryptoPriceCache _priceCache = priceCache;

    [HttpGet("Prices")]
    [AllowAnonymous]
    public IActionResult GetPrices()
    {
        return Ok(_priceCache.GetAll());
    }
}

