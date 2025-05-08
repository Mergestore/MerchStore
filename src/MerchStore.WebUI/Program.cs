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
using MerchStore.WebUI.Models;
using MerchStore.WebUI.Models.Auth; // För Json-konvertering




// Skapa en WebApplicationBuilder som är startpunkten för att konfigurera applikationen
var builder = WebApplication.CreateBuilder(args);

    // 🔐 Lägg till API-nyckel-autentisering
    builder.Services.AddAuthentication()
        .AddApiKey(builder.Configuration["ApiKey:Value"] 
        ?? throw new InvalidOperationException("API Key is not configured in appsettings."));

    // 🔐 Lägg till en policy som kräver att man är autentiserad via vår API-nyckel
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("ApiKeyPolicy", policy =>
            policy.AddAuthenticationSchemes(ApiKeyAuthenticationDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser());
    });

// Lägg till MVC-stöd med Controllers och Views
builder.Services.AddControllersWithViews();

// Konfigurera cookie-baserad autentisering
// Detta sätter upp cookies som mekanismen för att hålla användare inloggade
// Konfigurera cookie-baserad autentisering med förbättrad säkerhet
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Cookie-inställningar
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.Name = "MerchStore.Auth";

        // Utgångsinställningar
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;

        // Autentiseringssökvägar
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// Konfigurera auktorisering med tydliga policyer
builder.Services.AddAuthorization(options =>
{
    // Använd const från UserRoles
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole(UserRoles.Administrator));

    options.AddPolicy("CustomerOnly", policy =>
        policy.RequireRole(UserRoles.Customer));
        
    // Policy som accepterar både admin och kund
    options.AddPolicy("AuthenticatedUsers", policy =>
        policy.RequireRole(UserRoles.Administrator, UserRoles.Customer));
});

// Detta registrerar en CORS-policy som tillåter alla domäner, headers och metoder
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAllOrigins",
            builder =>
            {
                builder.AllowAnyOrigin()  // Vem som helst får anropa (⚠️ i produktion: begränsa!)
                    .AllowAnyHeader()  // Tillåt alla typer av headers
                    .AllowAnyMethod(); // Tillåt GET, POST, PUT, DELETE etc
            });
    });

// Lägg till minnescache för sessioner
builder.Services.AddDistributedMemoryCache();

// Konfigurera sessionshantering (används för kundvagn)
builder.Services.AddSession(options =>
{
    // Hur länge en session är aktiv
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    
    // Förhindra klientskript från att komma åt sessionscookien
    options.Cookie.HttpOnly = true;
    
    // Markera cookien som nödvändig (för GDPR-samtycke)
    options.Cookie.IsEssential = true;
});

// Lägg till HttpContextAccessor för att tjänster ska kunna komma åt HTTP-context
builder.Services.AddHttpContextAccessor();

// Registrera kundvagnstjänsten för dependency injection
builder.Services.AddScoped<CartSessionService>();

// Registrera autentiseringstjänsten för dependency injection
builder.Services.AddScoped<AuthService>();

// Lägg till Application-lagrets tjänster (från Application-projektet)
builder.Services.AddApplication();

// Lägg till Infrastructure-lagrets tjänster (från Infrastructure-projektet)
// Inkluderar databaskoppling, repositories, etc.
MerchStore.Infrastructure.DependencyInjection.AddInfrastructure(builder.Services, builder.Configuration);

// Konfigurera stöd för API-dokumentation
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy(); // för objekt
        options.JsonSerializerOptions.DictionaryKeyPolicy = new JsonSnakeCaseNamingPolicy();   // för dictionaries
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());           // gör enum till string istället för siffror
    });


// Lägg till Swagger för API-dokumentation
builder.Services.AddSwaggerGen(options =>
{
    // Grundinformation om API:et
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MerchStore API",
        Version = "v1",
        Description = "API for MerchStore product catalog",
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
    
        // 🔐 Lägg till API-nyckel-stöd i Swagger
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

// Program.cs - Lägg till loggning för anslutningssträngen
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"Använder anslutningssträng: {connectionString}");

//builder.Services.AddInfrastructure(builder.Configuration);

//builder.Configuration.AddUserSecrets<Program>();

// Bygg applikationen med alla konfigurerade tjänster
var app = builder.Build();

// Konfigurera middleware för att hantera begärningar och svar
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Förbereder databasinitiering...");
        
        var context = services.GetRequiredService<AppDbContext>();
        
        // Kontrollera anslutningen
        var canConnect = await context.Database.CanConnectAsync();
        logger.LogInformation($"Kan ansluta till databasen: {canConnect}");
        
        if (canConnect)
        {
            // Kör migrationer
            logger.LogInformation("Applicerar migrationer...");
            await context.Database.MigrateAsync();
            
            // Seeda databasen
            logger.LogInformation("Startar seeding...");
            var seeder = services.GetRequiredService<AppDbContextSeeder>();
            await seeder.SeedAsync();
            logger.LogInformation("Seeding slutförd");
        }
        else
        {
            logger.LogError("Kunde inte ansluta till databasen. Skipping migrations och seeding.");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ett fel uppstod vid initiering av databasen.");
    }
}

// Applicera migrationer automatiskt vid uppstart
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
        
        // Seed-databasen efter migrering
        await services.SeedDatabaseAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ett fel uppstod vid migrering av databasen.");
    }
}

// Konfigurera HTTP-request-pipelinen baserat på miljö (utveckling/produktion)
if (!app.Environment.IsDevelopment())
{
    
    // I utvecklingsmiljö, fyll databasen med testdata
    app.Services.SeedDatabaseAsync().Wait();

    // Aktivera Swagger UI för API-testning i utvecklingsmiljö
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MerchStore API V1");
    });
    
    // Kommenterade bort detta  bara
    // I produktion, använd en generisk felsida
   // app.UseExceptionHandler("/Home/Error");
    
    // Aktivera HSTS för säkrare HTTPS-anslutningar
    //app.UseHsts();
}
else
{
    // I utvecklingsmiljö, fyll databasen med testdata
    app.Services.SeedDatabaseAsync().Wait();

    // Aktivera Swagger UI för API-testning i utvecklingsmiljö
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MerchStore API V1");
    });
}


app.UseCors("AllowAllOrigins");

// Omdirigera HTTP-trafik till HTTPS
app.UseHttpsRedirection();

// Aktivera sessionshantering
app.UseSession();

// Konfigurera routing
app.UseRouting();

app.UseCors("AllowAllOrigins");


// Aktivera autentisering (vem användaren är)
app.UseAuthentication();

// Aktivera auktorisering (vad användaren får göra)
app.UseAuthorization();

app.UseStaticFiles();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Starta applikationen
app.Run();