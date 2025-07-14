using Microsoft.EntityFrameworkCore;
using SteamBoxd.Models;

namespace SteamBoxd.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Game> Games { get; set; }
    }
}
#pragma warning restore format