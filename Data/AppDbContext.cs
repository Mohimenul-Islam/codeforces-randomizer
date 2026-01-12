using CodeforcesRandomizer.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeforcesRandomizer.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<PracticeGroup> PracticeGroups => Set<PracticeGroup>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).HasMaxLength(256);
            entity.Property(u => u.CodeforcesHandle).HasMaxLength(24);
        });

        modelBuilder.Entity<PracticeGroup>(entity =>
        {
            entity.HasIndex(g => new { g.UserId, g.Name }).IsUnique();
            entity.Property(g => g.Name).HasMaxLength(50);
            entity.HasOne(g => g.User)
                .WithMany()
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
