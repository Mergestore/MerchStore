using MerchStore.WebUI.Services;

namespace MerchStore.WebUI.Infrastructure.ServiceExtensions;

/// <summary>
/// Innehåller tilläggsmetoder för att konfigurera sessionshantering i applikationen.
/// </summary>
public static class SessionExtensions
{
    /// <summary>
    /// Lägger till sessionstjänster i applikationen.
    /// Detta inkluderar minnescache, sessionskonfiguration och tjänster som är beroende av sessioner.
    /// </summary>
    /// <param name="services">Tjänstsamlingen att lägga till tjänster i</param>
    /// <returns>Tjänstsamlingen för kedjning</returns>
    public static IServiceCollection AddSessionServices(this IServiceCollection services)
    {
        // Lägg till minnescache för sessioner
        services.AddDistributedMemoryCache();

        // Konfigurera sessionshantering (används för kundvagn)
        services.AddSession(options =>
        {
            // Hur länge en session är aktiv
            options.IdleTimeout = TimeSpan.FromMinutes(30);

            // Förhindra klientskript från att komma åt sessionscookien
            options.Cookie.HttpOnly = true;

            // Markera cookien som nödvändig (för GDPR-samtycke)
            options.Cookie.IsEssential = true;
        });

        // Lägg till HttpContextAccessor för att tjänster ska kunna komma åt HTTP-context
        services.AddHttpContextAccessor();

        // Registrera kundvagnstjänsten för beroendeinjektion
        services.AddScoped<CartSessionService>();

        return services;
    }
}