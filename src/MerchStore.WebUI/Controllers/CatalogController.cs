using MerchStore.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MerchStore.WebUI.Models.Catalog;

namespace MerchStore.WebUI.Controllers;

public class CatalogController : Controller
{
    private readonly ICatalogService _catalogService;
    private readonly IReviewService _reviewService; // Lägg till referensen
    private readonly ILogger<CatalogController> _logger;

    public CatalogController(ICatalogService catalogService, IReviewService reviewService, ILogger<CatalogController> logger) // Uppdatera konstruktorn
    {
        _catalogService = catalogService;
        _reviewService = reviewService; // Tilldela tjänsten
        _logger = logger;
    }

    // GET: Catalog
public async Task<IActionResult> Index()
{
    try
    {
        // Get all products from the service
        var products = await _catalogService.GetAllProductsAsync();

        // Map domain entities to view models
        var productViewModels = new List<ProductCardViewModel>();
        
        foreach (var product in products)
        {
            // Hämta recensionsdata för varje produkt
            double averageRating = 0;
            int reviewCount = 0;
            
            try
            {
                averageRating = await _reviewService.GetAverageRatingForProductAsync(product.Id);
                reviewCount = await _reviewService.GetReviewCountForProductAsync(product.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Kunde inte hämta recensionsdata för produkt {ProductId}", product.Id);
                // Fortsätt med defaultvärden
            }
            
            productViewModels.Add(new ProductCardViewModel
            {
                Id = product.Id,
                Name = product.Name,
                TruncatedDescription = product.Description.Length > 100
                    ? product.Description.Substring(0, 97) + "..."
                    : product.Description,
                FormattedPrice = product.Price.ToString(),
                PriceAmount = product.Price.Amount,
                ImageUrl = product.ImageUrl?.ToString(),
                StockQuantity = product.StockQuantity,
                AverageRating = averageRating,
                ReviewCount = reviewCount
            });
        }

        // Skapa viewmodel
        var viewModel = new ProductCatalogViewModel
        {
            FeaturedProducts = productViewModels
        };

        return View(viewModel);
    }
    catch (Exception ex)
    {
        // Log the exception
        Console.WriteLine($"Error in ProductCatalog: {ex.Message}");

        // Show an error message to the user
        ViewBag.ErrorMessage = "An error occurred while loading products. Please try again later.";
        return View("Error");
    }
}

    // GET: Store/Details/5
    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            // Get the specific product from the service
            var product = await _catalogService.GetProductByIdAsync(id);

            // Return 404 if product not found
            if (product is null)
            {
                return NotFound();
            }

            // Hämta recensionsdata - omslut med try/catch för att hantera eventuella fel
            try
            {
                ViewBag.AverageRating = await _reviewService.GetAverageRatingForProductAsync(id);
                ViewBag.ReviewCount = await _reviewService.GetReviewCountForProductAsync(id);
            }
            catch (Exception reviewEx)
            {
                // Logga felet men fortsätt - visa produkten ändå
                Console.WriteLine($"Error fetching reviews: {reviewEx.Message}");
                ViewBag.AverageRating = 0;
                ViewBag.ReviewCount = 0;
            }

            // Map domain entity to view model
            var viewModel = new ProductDetailsViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                FormattedPrice = product.Price.ToString(),
                PriceAmount = product.Price.Amount,
                ImageUrl = product.ImageUrl?.ToString(),
                StockQuantity = product.StockQuantity
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Error in ProductDetails: {ex.Message}");

            // Show an error message to the user
            ViewBag.ErrorMessage = "An error occurred while loading the product. Please try again later.";
            return View("Error");
        }
    }
}