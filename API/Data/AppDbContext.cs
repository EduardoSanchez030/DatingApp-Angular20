using Microsoft.EntityFrameworkCore;
using DatingApp.API.Entities;
using API.Entities;

namespace DatingApp.API.Data
{
    public class AppDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<AppUser> Users { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Photo> Photos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Add your entity configurations here
        }
    }
}