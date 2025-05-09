using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MerchStore.Domain.Entities;
using MerchStore.Domain.ValueObjects;

namespace MerchStore.Infrastructure.Persistence;

/// <summary>
/// Används för att fylla databasen med initial data för utveckling, testning och demo
/// </summary>
public class AppDbContextSeeder
{
    private readonly ILogger<AppDbContextSeeder> _logger;
    private readonly AppDbContext _context;

    /// <summary>
    /// Constructor that accepts the context and a logger
    /// </summary>
    /// <param name="context">The database context to seed</param>
    /// <param name="logger">The logger for logging seed operations</param>
    public AppDbContextSeeder(AppDbContext context, ILogger<AppDbContextSeeder> logger)
    {
        _context = context;
        _logger = logger;
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
            _logger.LogError(ex, "Ett fel uppstod vid seedning av databasen.");
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
}