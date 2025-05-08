using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MerchStore.Infrastructure.Persistence;

/// <summary>
/// Används av EF Core för att skapa databaskontext vid design-time (t.ex. vid migreringar)
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Hitta WebUI-projektets katalog (en nivå upp och sedan in i WebUI)
        var webUiPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "MerchStore.WebUI"));

        var configuration = new ConfigurationBuilder()
            .SetBasePath(webUiPath)
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.Development.json", optional: true)
            .Build();

        var builder = new DbContextOptionsBuilder<AppDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        builder.UseSqlServer(connectionString);

        return new AppDbContext(builder.Options);
    }
}