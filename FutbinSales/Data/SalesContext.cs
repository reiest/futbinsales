using Microsoft.EntityFrameworkCore;
using FutbinSales.Core.Players;

namespace FutbinSales.Data;
public class SalesContext : DbContext
{
    public SalesContext(DbContextOptions<SalesContext> options) : base(options)
    {
    }
    
    public DbSet<Player> Players { get; set; } = null!;
    public DbSet<Sale> Sales { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>()
            .HasMany(p => p.Sales)
            .WithOne(s => s.Player)
            .HasForeignKey(s => s.PlayerId);
    }
}