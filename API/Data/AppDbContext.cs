using Microsoft.EntityFrameworkCore;
using DatingApp.API.Entities;

namespace DatingApp.API.Data
{
    public class AppDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<AppUser> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Add your entity configurations here
        }
    }
}