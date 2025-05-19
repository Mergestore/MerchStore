using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MerchStore.Application.Common.Interfaces;
using MerchStore.Domain.Interfaces;
using MerchStore.Infrastructure.Persistence;
using MerchStore.Infrastructure.Persistence.Repositories;

// 👇 behövs för att registrera externa API-tjänster
using MerchStore.Infrastructure.ExternalServices.Reviews;
using MerchStore.Infrastructure.ExternalServices.Reviews.Configurations;

namespace MerchStore.Infrastructure;

/// <summary>
/// Innehåller tilläggsmetoder för att registrera infrastrukturlagrets tjänster i beroendeinjektionscontainern.
/// Detta håller all registreringslogik på ett ställe och gör den återanvändbar.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registrerar alla nödvändiga tjänster för infrastrukturlagret i DI-containern.
    /// Detta inkluderar:
    /// - Databaskontext (SQL Server)
    /// - Repositories för dataåtkomst
    /// - Unit of Work för transaktionshantering
    /// - Repository Manager för att hantera repositories
    /// - Loggning
    /// - Databasseedning
    /// </summary>
    /// <param name="services">Tjänstsamlingen att lägga till tjänster i</param>
    /// <param name="configuration">Konfigurationen för databasanslutningssträngar</param>
    /// <returns>Tjänstsamlingen för kedjning</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Hämta anslutningssträngen från konfigurationen, SQL Server konfiguration
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Konfigurera SQL Server som databas
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // Registrera repositories för dataåtkomst
        // Scoped betyder att en ny instans skapas per HTTP-request
        services.AddScoped<IProductRepository, ProductRepository>();

        // Registrera Unit of Work för att hantera transaktioner
        // Detta säkerställer att alla databasoperationer i en transaktion antingen lyckas eller misslyckas tillsammans
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Registrera Repository Manager för att hantera alla repositories
        // Detta ger en central punkt för att hantera alla databasoperationer
        services.AddScoped<IRepositoryManager, RepositoryManager>();

        // Lägg till loggningstjänster om de inte redan är tillagda
        services.AddLogging();

        // Registrera databasseedning för att fylla databasen med initial data
        services.AddScoped<AppDbContextSeeder>();

        
        // ✨ Review API-tjänster
        services.Configure<ReviewApiOptions>(configuration.GetSection(ReviewApiOptions.SectionName));
        services.AddHttpClient<ReviewApiClient>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5)); // för återanvändning

        services.AddSingleton<MockReviewService>(); // används som fallback
        services.AddScoped<IReviewRepository, ExternalReviewRepository>(); // Repository med Circuit Breaker

        return services;
    }

    /// <summary>
    /// Fyller databasen med initial data.
    /// Detta är en tilläggsmetod på IServiceProvider som kan anropas från Program.cs.
    /// Metoden skapar en ny scope för att hantera beroenden och kör sedan seedningen.
    /// </summary>
    /// <param name="serviceProvider">Service provider för att lösa beroenden</param>
    /// <returns>En task som representerar den asynkrona operationen</returns>
    public static async Task SeedDatabaseAsync(this IServiceProvider serviceProvider)
    {
        // Skapa en ny scope för att hantera beroenden
        using var scope = serviceProvider.CreateScope();
        // Hämta seedern från DI-containern
        var seeder = scope.ServiceProvider.GetRequiredService<AppDbContextSeeder>();
        // Kör seedningen
        await seeder.SeedAsync();
    }
}
