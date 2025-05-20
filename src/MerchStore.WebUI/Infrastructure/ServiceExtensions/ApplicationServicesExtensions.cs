using System.Text.Json.Serialization;
using MerchStore.WebUI.Models;
using MerchStore.WebUI.Services;

namespace MerchStore.WebUI.Infrastructure.ServiceExtensions;

/// <summary>
/// Innehåller tilläggsmetoder för att konfigurera applikationstjänster i applikationen.
/// </summary>
public static class ApplicationServicesExtensions
{
    /// <summary>
    /// Lägger till MVC och applikationsspecifika tjänster i applikationen.
    /// Konfigurerar JSON-serialisering och registrerar applikationstjänster.
    /// </summary>
    /// <param name="services">Tjänstsamlingen att lägga till tjänster i</param>
    /// <returns>Tjänstsamlingen för kedjning</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Lägg till MVC-stöd med Controllers och Views
        services.AddControllersWithViews()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy(); // för objekt
                options.JsonSerializerOptions.DictionaryKeyPolicy = new JsonSnakeCaseNamingPolicy();   // för dictionaries
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());           // gör enum till string istället för siffror
            });

        // Registrera autentiseringstjänsten för beroendeinjektion
        services.AddScoped<AuthService>();

        return services;
    }
}