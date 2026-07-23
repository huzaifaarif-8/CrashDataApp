using CrashDataApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CrashDataApp.Data;

public class CrashContext : DbContext
{
    public CrashContext(DbContextOptions<CrashContext> options) : base(options) { }

    public DbSet<Crash> Crashes => Set<Crash>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Crash>(entity =>
        {
            entity.ToTable("Crashes");
            entity.HasIndex(c => c.Year);
            entity.HasIndex(c => c.Operator);
        });
    }
}
