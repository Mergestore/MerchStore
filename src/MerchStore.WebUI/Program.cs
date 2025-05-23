using System.Reflection;
using MerchStore.Application;
using MerchStore.Infrastructure;
using MerchStore.Infrastructure.Persistence;
using MerchStore.WebUI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MerchStore.WebUI.Authentication.ApiKey;
using MerchStore.WebUI.Infrastructure;
using System.Text.Json.Serialization;
using MerchStore.Infrastructure.Models.Auth;
using MerchStore.WebUI.Models;
using MerchStore.WebUI.Models.Auth;
using Microsoft.AspNetCore.Identity;
using MerchStore.WebUI.Infrastructure.ServiceExtensions;
using Azure.Identity;

/// <summary>
/// MerchStore Application - Startpunkt och konfigurera ASP.NET Core-applikation
/// 
/// UPPDATERAD MED KEY VAULT INTEGRATION
/// ===================================
/// Nu använder applikationen Azure Key Vault för att hantera känsliga konfigurationsvärden
/// som connection strings, API-nycklar och lösenord. Detta ökar säkerheten betydligt
/// jämfört med att lagra hemliga värden i konfigurationsfiler.
/// 
/// Key Vault Integration:
/// ----------------------
/// * DefaultAzureCredential: Automatisk autentisering som fungerar både lokalt och i Azure
/// * Lokalt: Använder Azure CLI credentials eller Visual Studio credentials
/// * Azure: Använder Managed Identity för säker åtkomst utan lösenord
/// * Konfigurationsvärden hämtas transparent från Key Vault vid runtime
/// 
/// Applikationsarkitektur:
/// -----------------------
/// * Clean Architecture med separation av ansvar:
///   - WebUI: Presentationslagret (Controllers, Views)
///   - Application: Applikationslogik (Services, ViewModels)
///   - Infrastructure: Dataåtkomst och externa tjänster
///   - Domain: Domänmodeller och affärslogik
/// 
/// Användarhantering och Autentisering:
/// -----------------------------------
/// * ASP.NET Core Identity:
///   - Använder ApplicationUser som utökar IdentityUser
///   - Lagrar användardata i SQL Server (AspNetUsers-tabellen)
///   - Hanterar roller via UserRoles-klassen (Admin/Customer)
///
/// * Autentiseringssystem:
///   - Cookie-baserad autentisering för webbsidor
///   - API-nyckel autentisering för API-anrop
///   - Konfigureras i AuthenticationExtensions.cs
///
/// * Användarroller och behörigheter:
///   - Administrator: Åtkomst till admin-gränssnitt och CRUD-operationer
///   - Customer: Standardroll för alla registrerade användare
///   - Rollhantering via _userManager.AddToRoleAsync() och User.IsInRole()
///   - [Authorize]-attribut för att skydda controllers och actions
///
/// * Seed av roller och användare:
///   - Initiala roller och användardata skapas via SeedRolesAndUsersAsync()
///   - Admin-konto skapas automatiskt vid uppstart
///   - Test-konton för utveckling skapas i utvecklingsmiljö
///
/// Applikationsflöde:
/// -----------------
/// 1. Key Vault-konfiguration läses in först
/// 2. Konfigurering av tjänster (AddAuthenticationServices, AddSessionServices, etc.)
/// 3. Databasinitiering och seedning
/// 4. Konfigurering av HTTP-pipeline med middleware
/// 5. Routing och körning av applikationen
/// </summary>

// Skapa en WebApplicationBuilder som är startpunkten för att konfigurera applikationen
var builder = WebApplication.CreateBuilder(args);

// Key vault URI hämtas från konfigurationen
var keyVaultUri = builder.Configuration["KeyVault:Uri"];

if (!string.IsNullOrEmpty(keyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUri),
        new DefaultAzureCredential());
        
    // Logga att Key Vault används (men visa inte känslig information)
    Console.WriteLine($"🔐 Key Vault konfigurerad: {keyVaultUri}");
}
else
{
    Console.WriteLine("WARNING: Key Vault URI inte konfigurerad - använder lokala konfigurationsfiler");
}

// Lägger till services/tjänster från src/MerchStore.WebUI/Infrastructure/ServiceExtensions

// Lägg till autentisering och auktoriseringstjänster
builder.Services.AddAuthenticationServices(builder.Configuration);

// Lägg till CORS-tjänster
builder.Services.AddCorsServices();

// Lägg till sessionstjänster och kundvagn
builder.Services.AddSessionServices();

// Lägg till MVC och applikationsspecifika tjänster
builder.Services.AddApplicationServices();

// Lägg till Swagger-tjänster
builder.Services.AddSwaggerServices();

// Lägg till applikationslagrets tjänster
builder.Services.AddApplication();

// Lägg till infrastrukturlagrets tjänster (databas, repositories, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Logga anslutningssträngen (men bara längden av säkerhetsskäl)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"📊 Använder anslutningssträng med längd: {connectionString?.Length ?? 0} tecken");

// 🔍 Diagnostik för Key Vault (endast i utvecklingsmiljö)
if (builder.Environment.IsDevelopment())
{
    // Testa att några viktiga konfigurationsvärden finns
    var hasConnectionString = !string.IsNullOrEmpty(builder.Configuration.GetConnectionString("DefaultConnection"));
    var hasApiKey = !string.IsNullOrEmpty(builder.Configuration["ApiKey:Value"]);
    var hasReviewApiKey = !string.IsNullOrEmpty(builder.Configuration["ReviewApi:ApiKey"]);
    
    Console.WriteLine($"🔧 Utvecklingsdiagnostik:");
    Console.WriteLine($"   - Connection String: {(hasConnectionString ? "Finns" : "Saknas")}");
    Console.WriteLine($"   - API Key: {(hasApiKey ? "Finns" : "Saknas")}");
    Console.WriteLine($"   - Review API Key: {(hasReviewApiKey ? "Finns" : "Saknas")}");
}

// Bygg applikationen med alla konfigurerade tjänster
var app = builder.Build();

// Initiera och seeda databasen
await app.InitializeDatabaseAsync();

// Konfigurera HTTP-request-pipelinen baserat på miljö (utveckling/produktion)
if (!app.Environment.IsDevelopment())
{
    // Seeda databasen med data
    await app.Services.SeedDatabaseAsync();
    await app.Services.SeedRolesAndUsersAsync();

    // Aktivera Swagger UI för API-testning
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MerchStore API V1");
    });

    // Aktivera HSTS för säkrare HTTPS-anslutningar
    app.UseHsts();
}
else
{
    // I utvecklingsmiljö, fyll databasen med testdata
    await app.Services.SeedDatabaseAsync();
    await app.Services.SeedRolesAndUsersAsync();

    // Aktivera Swagger UI för API-testning
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MerchStore API V1");
    });
}

// Omdirigera HTTP-trafik till HTTPS
app.UseHttpsRedirection();

// Aktivera sessionshantering
app.UseSession();

// Aktivera CORS
app.UseCors("AllowAllOrigins");

// Konfigurera routing
app.UseRouting();

// Aktivera autentisering (vem användaren är)
app.UseAuthentication();

// Aktivera auktorisering (vad användaren får göra)
app.UseAuthorization();

// Aktivera statiska filer
app.UseStaticFiles();

// Konfigurera rutter
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Starta applikationen
app.Run();