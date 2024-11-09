using Core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Core.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<PrimeCheckHistory> PrimeCheckHistory { get; set; }
        public DbSet<Models.CancellationToken> CancellationToken { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<PrimeCheckHistory>().ToTable("PrimeCheckHistory");
            builder.Entity<Models.CancellationToken>().ToTable("CancellationToken");
        }
    }
}