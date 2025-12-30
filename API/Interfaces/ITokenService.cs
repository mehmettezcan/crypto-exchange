

using API.Data.Entities;

namespace API.Interfaces;

public interface ITokenService
{
    string CreateToken(User user);
}