using API.Data;
using API.Data.Entities;
using API.Interfaces;

namespace API.Repositories;

public class UserRepository(DataContext context) : IUserRepository
{
    readonly DataContext _context = context;

    public Task<User?> GetByUsernameAsync(string username)
    {
        var user = _context.Users.FirstOrDefault(u => u.Username.Equals(username));
        return Task.FromResult(user);
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email.Equals(email));
        return Task.FromResult(user);
    }

    public Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
        return Task.FromResult(user);
    }

    public Task<bool> ExistsAsync(string username, string email)
    {
        var exists = _context.Users.Any(u =>
            u.Username.Equals(username) ||
            u.Email.Equals(email));
        return Task.FromResult(exists);
    }
}

