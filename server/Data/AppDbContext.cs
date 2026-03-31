using Microsoft.EntityFrameworkCore;
using AdventureRpg.Models;

namespace AdventureRpg.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<Item> Items => Set<Item>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Character>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired().HasMaxLength(24);
            e.Property(c => c.Class).HasConversion<string>();
        });

        modelBuilder.Entity<Item>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.Name).IsRequired().HasMaxLength(100);
            e.Property(i => i.Type).HasConversion<string>();
        });
    }
}
