using API.Controllers.DTOs;
using API.Data.Entities;
using API.Interfaces;

namespace API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        Console.WriteLine("LoginAsync called with username: " + request.Username + " and password: " + request.Password);
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user == null)
        {
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        var token = _tokenService.CreateToken(user);

        return new AuthResponse
        {
            Token = token
        };
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        var exists = await _userRepository.ExistsAsync(request.Username, request.Email);
        if (exists)
        {
            return null;
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);

        var token = _tokenService.CreateToken(user);

        return new AuthResponse
        {
            Token = token,
        };
    }
}

