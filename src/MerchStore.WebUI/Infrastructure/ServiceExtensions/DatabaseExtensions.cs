using Microsoft.EntityFrameworkCore;
using MerchStore.Infrastructure.Persistence;
using MerchStore.Infrastructure;

namespace MerchStore.WebUI.Infrastructure.ServiceExtensions;

/// <summary>
/// Innehåller tilläggsmetoder för att initiera och konfigurera databasen i applikationen.
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Initierar databasen genom att köra migrationer och fylla den med initialdata.
    /// Metoden kontrollerar först att anslutningen fungerar, kör migrationer och sedan seeding.
    /// </summary>
    /// <param name="app">Webapplikationen att initiera databasen för</param>
    /// <returns>En task som representerar den asynkrona operationen</returns>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Förbereder databasinitiering...");
            var context = services.GetRequiredService<AppDbContext>();

            // Kontrollera anslutningen
            var canConnect = await context.Database.CanConnectAsync();
            logger.LogInformation($"Kan ansluta till databasen: {canConnect}");

            if (canConnect)
            {
                // Kör migrationer automatiskt
                logger.LogInformation("Applicerar migrationer...");
                await context.Database.MigrateAsync(); // Använd Migrate istället för EnsureCreated

                // Seeda databasen
                logger.LogInformation("Startar seeding...");
                await services.SeedDatabaseAsync();
                logger.LogInformation("Seeding slutförd");
            }
            else
            {
                logger.LogError("Kunde inte ansluta till databasen. Hoppar över migrationer och seeding.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ett fel uppstod vid initiering av databasen.");
        }
    }
}