using Microsoft.EntityFrameworkCore;
using KPWrestlingScoreboard.Models;

namespace KPWrestlingScoreboard.Data
{
    public class WrestlingDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Wrestler> Wrestlers { get; set; }
        public DbSet<WeightCategory> WeightCategories { get; set; }
        public DbSet<Models.Region> Regions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                "Server=DESKTOP-4K729EO;" +
                "Database=kokos;" +
                "Integrated Security=True;" +
                "TrustServerCertificate=True;" +
                "Connect Timeout=30;",
                options => options.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.IdRole);

            modelBuilder.Entity<Wrestler>()
                .HasOne(w => w.WeightCategory)
                .WithMany(wc => wc.Wrestlers)
                .HasForeignKey(w => w.IdWeightCategory);

            modelBuilder.Entity<Wrestler>()
                .HasOne(w => w.Region)
                .WithMany(r => r.Wrestlers)
                .HasForeignKey(w => w.IdRegion);
        }
    }
}
