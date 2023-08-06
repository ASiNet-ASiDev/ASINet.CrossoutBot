using ASiNet.Crossaut.DB.Domain.News;
using Microsoft.EntityFrameworkCore;

namespace ASiNet.Crossaut.DB;

public class CrossautNewsDBContext : DbContext
{

    public DbSet<DbImageUri> Images { get; set; } = null!;
    public DbSet<DbNews> News { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if(!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"database")))
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"database"));
        optionsBuilder.UseLazyLoadingProxies();
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"database", "database.db");
        optionsBuilder.UseSqlite($"Data Source={path}");
        base.OnConfiguring(optionsBuilder);
    }
}
