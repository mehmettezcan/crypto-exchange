

using API.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        
    }
}