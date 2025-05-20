using MerchStore.Infrastructure.Models.Auth;
using MerchStore.WebUI.Authentication.ApiKey;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace MerchStore.WebUI.Infrastructure.ServiceExtensions;

/// <summary>
/// Innehåller tilläggsmetoder för att hantera autentisering och auktorisering i applikationen.
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// Lägger till alla autentiseringstjänster som applikationen behöver.
    /// 
    /// Autentiseringssystemet i applikationen:
    /// ----------------------------------------
    /// 1. ASP.NET Core Identity: 
    ///    - Hanterar användare, lösenord, roller och claims
    ///    - Lagrar data i databasmodeller (AspNetUsers, AspNetRoles, etc.)
    ///    - Hanterar lösenordspolicies (längd, komplexitet)
    ///    - Erbjuder inbyggt stöd för verifiering och återställning
    /// 
    /// 2. Cookie-autentisering:
    ///    - När användare loggar in skapas en säker cookie
    ///    - Denna cookie identifierar användaren vid varje request
    ///    - Konfigurerad med säkerhetsregler (HttpOnly, expiration, etc.)
    /// 
    /// 3. API-nyckel autentisering:
    ///    - Används för externa tjänster som API-anrop
    ///    - Separat från användarautentisering
    /// 
    /// 4. Auktoriseringspolicies:
    ///    - Definierar vem som får göra vad
    ///    - Roller används för att gruppera behörigheter (Admin/Customer)
    ///    - Policyn "AdminOnly" kontrollerar Admin-roller
    /// 
    /// Användningsexempel i controllers:
    /// - [Authorize] - Kräver inloggad användare
    /// - [Authorize(Roles = "Admin")] - Kräver Admin-roll
    /// - [Authorize(Policy = "AdminOnly")] - Använder policyn
    /// </summary>
    /// <param name="services">Tjänstsamlingen att lägga till tjänster i</param>
    /// <param name="configuration">Konfigurationen för att hämta API-nycklar och andra inställningar</param>
    /// <returns>Tjänstsamlingen för kedjning</returns>
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Lägg till API-nyckel-autentisering
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddApiKey(configuration["ApiKey:Value"]
                ?? throw new InvalidOperationException("API-nyckel är inte konfigurerad i appsettings."));

        // Lägg till auktoriseringspolicys
        services.AddAuthorization(options =>
        {
            options.AddPolicy("ApiKeyPolicy", policy =>
                policy.AddAuthenticationSchemes(ApiKeyAuthenticationDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser());

            // Lägg till AdminOnly-policy som kräver Admin-rollen
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole("Admin"));
        });

        // Lägg till ASP.NET Core Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Lösenordsinställningar
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;

                // Låsningsinställningar
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;

                // Användarinställningar
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<MerchStore.Infrastructure.Persistence.AppDbContext>()
            .AddDefaultTokenProviders();

        // Konfigurera cookie-inställningar
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(1);
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.SlidingExpiration = true;
        });

        return services;
    }
}