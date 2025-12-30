using API.Controllers.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Username and password are required" });
        }

        var response = await _authService.LoginAsync(request);
        if (response == null)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }


        return Ok(response);
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || 
            string.IsNullOrWhiteSpace(request.Email) || 
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Username, email, and password are required" });
        }

        if (request.Password.Length < 6)
        {
            return BadRequest(new { message = "Password must be at least 6 characters" });
        }

        var response = await _authService.RegisterAsync(request);
        if (response == null)
        {
            return Conflict(new { message = "Username or email already exists" });
        }

        return Ok(response);
    }
}

