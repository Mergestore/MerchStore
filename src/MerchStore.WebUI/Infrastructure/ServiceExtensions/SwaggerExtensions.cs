using System.Reflection;
using Microsoft.OpenApi.Models;
using MerchStore.WebUI.Authentication.ApiKey;

namespace MerchStore.WebUI.Infrastructure.ServiceExtensions;

/// <summary>
/// Innehåller tilläggsmetoder för att konfigurera Swagger/OpenAPI-dokumentation i applikationen.
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Lägger till Swagger/OpenAPI-dokumentationstjänster i applikationen.
    /// Konfigurerar API-information, XML-dokumentation och säkerhetsdefinitioner.
    /// </summary>
    /// <param name="services">Tjänstsamlingen att lägga till tjänster i</param>
    /// <returns>Tjänstsamlingen för kedjning</returns>
    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        // Konfigurera stöd för API-dokumentation
        services.AddEndpointsApiExplorer();

        // Lägg till Swagger för API-dokumentation
        services.AddSwaggerGen(options =>
        {
            // Grundinformation om API:et
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "MerchStore API",
                Version = "v1",
                Description = "API för MerchStore produktkatalog",
                Contact = new OpenApiContact
                {
                    Name = "MerchStore Support",
                    Email = "support@merchstore.example.com"
                }
            });

            // Inkludera XML-dokumentation från kodens XML-kommentarer
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // Lägg till API-nyckel-stöd i Swagger
            options.AddSecurityDefinition(ApiKeyAuthenticationDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Description = "Skriv in din API-nyckel här för att testa skyddade endpoints.",
                Name = ApiKeyAuthenticationDefaults.HeaderName, // X-API-Key
                In = ParameterLocation.Header, // Vi skickar nyckeln som en HTTP-header
                Type = SecuritySchemeType.ApiKey,
                Scheme = ApiKeyAuthenticationDefaults.AuthenticationScheme
            });

            // 🔐 Applicera säkerhetsfilter för endpoints med [Authorize]
            options.OperationFilter<MerchStore.WebUI.Infrastructure.SecurityRequirementsOperationFilter>();
        });

        return services;
    }
}