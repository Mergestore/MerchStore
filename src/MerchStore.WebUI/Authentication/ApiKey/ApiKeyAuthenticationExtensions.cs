using Microsoft.AspNetCore.Authentication;

namespace MerchStore.WebUI.Authentication.ApiKey;

/// <summary>
/// Förlängningsmetoder (extensions) för att enkelt registrera vår API-nyckel-autentisering i Program.cs.
/// </summary>
public static class ApiKeyAuthenticationExtensions
{
    /// <summary>
    /// Lägger till autentisering med möjlighet att konfigurera inställningar via lambda/metod.
    /// </summary>
    public static AuthenticationBuilder AddApiKey(
        this AuthenticationBuilder builder,
        Action<ApiKeyAuthenticationOptions>? configureOptions = null)
    {
        // Registrerar vårt autentiseringsschema med rätt handler och options-klass
        return builder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
            ApiKeyAuthenticationDefaults.AuthenticationScheme,
            configureOptions);
    }

    /// <summary>
    /// Lägger till autentisering där vi bara skickar in själva API-nyckeln direkt.
    /// </summary>
    public static AuthenticationBuilder AddApiKey(
        this AuthenticationBuilder builder,
        string apiKey)
    {
        // Vi använder första metoden men skickar med ett lambda-uttryck som sätter nyckeln
        return builder.AddApiKey(options => options.ApiKey = apiKey);
    }
}
