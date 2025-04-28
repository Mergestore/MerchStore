using System.Reflection;
using MerchStore.Application;
using MerchStore.Infrastructure;
using MerchStore.Infrastructure.Persistence;
using MerchStore.WebUI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

// Skapa en WebApplicationBuilder som är startpunkten för att konfigurera applikationen
var builder = WebApplication.CreateBuilder(args);

// Lägg till MVC-stöd med Controllers och Views
builder.Services.AddControllersWithViews();

// Konfigurera cookie-baserad autentisering
// Detta sätter upp cookies som mekanismen för att hålla användare inloggade
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Sökväg där användare omdirigeras om de inte är inloggade
        options.LoginPath = "/Account/Login";
        
        // Sökväg där användare skickas vid utloggning
        options.LogoutPath = "/Account/Logout";
        
        // Sökväg där användare skickas om de saknar behörighet
        options.AccessDeniedPath = "/Account/AccessDenied";
        
        // HttpOnly förhindrar att JavaScript får åtkomst till cookien (säkerhetsskydd)
        options.Cookie.HttpOnly = true;
        
        // Hur länge cookie/inloggning är giltig
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        
        // Förläng livstiden varje gång användaren interagerar med sidan
        options.SlidingExpiration = true;
    });

// Konfigurera auktorisering med rollbaserade policyer
builder.Services.AddAuthorization(options =>
{
    // Skapa en policy som kräver Admin-rollen
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    
    // Skapa en policy som kräver Customer-rollen
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
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
});

//builder.Services.AddInfrastructure(builder.Configuration);

//builder.Configuration.AddUserSecrets<Program>();

// Bygg applikationen med alla konfigurerade tjänster
var app = builder.Build();

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
    // I produktion, använd en generisk felsida
    app.UseExceptionHandler("/Home/Error");
    
    // Aktivera HSTS för säkrare HTTPS-anslutningar
    app.UseHsts();
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

// Omdirigera HTTP-trafik till HTTPS
app.UseHttpsRedirection();

// Aktivera sessionshantering
app.UseSession();

// Konfigurera routing
app.UseRouting();

// Aktivera autentisering (vem användaren är)
app.UseAuthentication();

// Aktivera auktorisering (vad användaren får göra)
app.UseAuthorization();

// Konfigurera statiska filer (CSS, JavaScript, bilder)
app.MapStaticAssets();

// Konfigurera standardrouting för MVC-kontroller
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Starta applikationen
app.Run();