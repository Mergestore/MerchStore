namespace MerchStore.WebUI.Infrastructure.ServiceExtensions;

/// <summary>
/// Innehåller tilläggsmetoder för att konfigurera CORS (Cross-Origin Resource Sharing) i applikationen.
/// </summary>
public static class CorsExtensions
{
    /// <summary>
    /// Lägger till CORS-tjänster i applikationen.
    /// Konfigurerar vilka domäner som kan anropa API:et, vilka headers som tillåts och vilka HTTP-metoder som stöds.
    /// </summary>
    /// <param name="services">Tjänstsamlingen att lägga till tjänster i</param>
    /// <returns>Tjänstsamlingen för kedjning</returns>
    public static IServiceCollection AddCorsServices(this IServiceCollection services)
    {
        // Lägg till CORS-policy som tillåter alla domäner, headers och metoder
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins",
                builder =>
                {
                    builder.AllowAnyOrigin()  // Vem som helst får anropa (i produktion: begränsa!)
                        .AllowAnyHeader()     // Tillåt alla typer av headers
                        .AllowAnyMethod();    // Tillåt GET, POST, PUT, DELETE etc
                });
        });

        return services;
    }
}