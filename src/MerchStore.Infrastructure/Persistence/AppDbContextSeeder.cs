using MerchStore.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MerchStore.Domain.Entities;
using MerchStore.Domain.ValueObjects;
using MerchStore.Infrastructure.Models.Auth;
using Microsoft.AspNetCore.Identity;

namespace MerchStore.Infrastructure.Persistence;


/// <summary>
/// Används för att fylla databasen med initial data för utveckling, testning och demo
/// </summary>
public class AppDbContextSeeder
{
    private readonly ILogger<AppDbContextSeeder> _logger;
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    /// <summary>
    /// Konstruktor som accepterar kontexten och en logger
    /// </summary>
    /// <param name="context">Databaskontexten som ska seedas</param>
    /// <param name="logger">Loggern för att logga seedningsoperationer</param>
    public AppDbContextSeeder(AppDbContext context,
        ILogger<AppDbContextSeeder> logger,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _logger = logger;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    /// <summary>
    /// Fyller databasen med initial data
    /// </summary>
    public virtual async Task SeedAsync()
    {
        try
        {
            // Säkerställ att databasen är skapad (behövs endast för in-memory databas)
            // För SQL Server används migrations istället
            await _context.Database.EnsureCreatedAsync();

            // Lägg till produkter om inga finns
            await SeedProductsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ett fel uppstod vid seedning av databasen eller rollerna.");
            throw;
        }
        try
        {
            // Seeda roller och användare
            await SeedRolesAndUsersAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ett fel uppstod vid seedning av roller och användare.");
            throw;
        }
    }

    /// <summary>
    /// Lägger till exempelprodukter i databasen
    /// </summary>
    private async Task SeedProductsAsync()
    {
        // Kontrollera om det redan finns produkter (för att undvika dubbletter)
        if (!await _context.Products.AnyAsync())
        {
            _logger.LogInformation("Lägger till produkter...");

            // Lägg till dina produkter
            var products = new List<Product>
            {
                // Keps - 1 bild
                new Product(
                    "Sportkeps",
                    "En snygg och bekväm sportkeps i svart med vår logotyp fram.",
                    new Uri("https://blobmerchstore2204.blob.core.windows.net/imagerepository/keps1.jpg"),
                    Money.FromSEK(249.99m),
                    75),

                // Proteinpulver - 1 bild
                new Product(
                    "Proteinpulver",
                    "Högkvalitativt proteinpulver med vaniljsmak, 900g.",
                    new Uri("https://blobmerchstore2204.blob.core.windows.net/imagerepository/proteinpulver1.jpg"),
                    Money.FromSEK(349.99m),
                    30),

                // Leggings - 2 bilder
                new Product(
                    "Träningsleggings",
                    "Högmidjade träningsleggings i svart med perfekt passform och stretchmaterial.",
                    new Uri("https://blobmerchstore2204.blob.core.windows.net/imagerepository/leggins1.jpg"),
                    Money.FromSEK(599.99m),
                    40),

                // Svart hoodie dam - 1 bild
                new Product(
                    "Hoodie Dam Svart",
                    "Mjuk och varm damhoodie i svart med logotyp på bröstet.",
                    new Uri("https://blobmerchstore2204.blob.core.windows.net/imagerepository/svarthoodiedam1.png"),
                    Money.FromSEK(599.99m),
                    25),

                // Svart hoodie man - 1 bild
                new Product(
                    "Hoodie Herr Svart",
                    "Bekväm herrhoodie i svart med dragkedja och logotyp på ryggen.",
                    new Uri("https://blobmerchstore2204.blob.core.windows.net/imagerepository/svarthoodieman1.jpeg"),
                    Money.FromSEK(599.99m),
                    25),

                // Vit hoodie man - 1 bild
                new Product(
                    "Hoodie Herr Vit",
                    "Stilren vit herrhoodie med logotyp på bröstet, perfekt för alla tillfällen.",
                    new Uri("https://blobmerchstore2204.blob.core.windows.net/imagerepository/vithoodieman1.jpeg"),
                    Money.FromSEK(599.99m),
                    25),

                // Vit hoodie dam - 1 bild
                new Product(
                    "Hoodie Dam Vit",
                    "Elegant vit damhoodie med logotyp, tillverkad av ekologisk bomull.",
                    new Uri("https://blobmerchstore2204.blob.core.windows.net/imagerepository/vithoodiedam1.png"),
                    Money.FromSEK(599.99m),
                    25),

                // Vit Tshirt man - 3 bilder
                new Product(
                    "T-shirt Herr Vit",
                    "Klassisk vit t-shirt för herr med rund halsringning och logotyp.",
                    new Uri("https://blobmerchstore2204.blob.core.windows.net/imagerepository/vittshirtman1.jpeg"),
                    Money.FromSEK(249.99m),
                    50)
            };

            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Produktseedning slutförd.");
        }
        else
        {
            _logger.LogInformation("Databasen innehåller redan produkter. Hoppar över produktseedning.");
        }
    }

    private async Task SeedRolesAndUsersAsync()
    {
        // Skapa roller om de inte redan finns
        if (!await _roleManager.RoleExistsAsync(UserRoles.Administrator))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.Administrator));
            _logger.LogInformation("Skapade Admin-rollen");
        }

        if (!await _roleManager.RoleExistsAsync(UserRoles.Customer))
        {
            await _roleManager.CreateAsync(new IdentityRole(UserRoles.Customer));
            _logger.LogInformation("Skapade Customer-rollen");
        }

        // skapa admin användare om den inte redan finns
        var adminUser = await _userManager.FindByNameAsync("admin");
        if (adminUser == null)
        {
            var newAdminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@merchstore.example.com",
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "Admin"
            };

            var result = await _userManager.CreateAsync(newAdminUser, "Admin123!");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(newAdminUser, UserRoles.Administrator);
                _logger.LogInformation("Skapade admin-användare och tilldelade Admin-rollen");
            }
            else
            {
                _logger.LogError("Kunde inte skapa admin-användaren: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Skapa en testanvändare om den inte redan finns
        var testUser = await _userManager.FindByNameAsync("testuser");
        if (testUser == null)
        {
            var newTestUser = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                EmailConfirmed = true,
                FirstName = "Test",
                LastName = "User"
            };

            var result = await _userManager.CreateAsync(newTestUser, "Test123!");
            if (result.Succeeded)
            {
                // Tilldela rollen till testanvändaren
                await _userManager.AddToRoleAsync(newTestUser, UserRoles.Customer);
                _logger.LogInformation("Skapade testanvändare och tilldelade Customer-rollen");
            }
            else
            {
                _logger.LogError("Kunde inte skapa testanvändaren: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}