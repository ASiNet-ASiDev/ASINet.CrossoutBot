using ASiNet.Crossaut.DB.Domain.Market;
using Microsoft.EntityFrameworkCore;

namespace ASiNet.Crossaut.DB;
public class CrossoutMarketDBContext : DbContext
{

    public DbSet<Item> Items { get; set; }

    public DbSet<Fraction> Fractions { get; set; }

    public DbSet<Rarity> Rarities { get; set; }

    public DbSet<Category> Categories { get; set; }

    public DbSet<MarketHistoryFrame> MarketHistory { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"database")))
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"database"));
        optionsBuilder.UseLazyLoadingProxies();
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"database", "market.db");
        optionsBuilder.UseSqlite($"Data Source={path}");
        base.OnConfiguring(optionsBuilder);
    }
}
