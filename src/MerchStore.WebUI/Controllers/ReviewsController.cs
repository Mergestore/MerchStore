using Microsoft.AspNetCore.Mvc;
using MerchStore.Application.Services.Interfaces;
using MerchStore.WebUI.Models;



namespace MerchStore.WebUI.Controllers;

/// <summary>
/// MVC-controller för att visa produktrecensioner.
/// </summary>
public class ReviewsController : Controller
{
    private readonly IReviewService _reviewService;
    private readonly ICatalogService _catalogService;

    public ReviewsController(IReviewService reviewService, ICatalogService catalogService)
    {
        _reviewService = reviewService;
        _catalogService = catalogService;
    }

    // GET: /Reviews
    public async Task<IActionResult> Index()
    {
        try
        {
            var products = await _catalogService.GetAllProductsAsync();
            var viewModel = new ProductReviewsViewModel
            {
                Products = products.ToList()
            };

            foreach (var product in viewModel.Products)
            {
                viewModel.ProductReviews[product.Id] = await _reviewService.GetReviewsByProductIdAsync(product.Id);
                viewModel.AverageRatings[product.Id] = await _reviewService.GetAverageRatingForProductAsync(product.Id);
                viewModel.ReviewCounts[product.Id] = await _reviewService.GetReviewCountForProductAsync(product.Id);
            }

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error fetching reviews: {ex.Message}";
            return View("Error");
        }
    }

    // GET: /Reviews/Product/{id}
    public async Task<IActionResult> Product(Guid id)
    {
        try
        {
            var product = await _catalogService.GetProductByIdAsync(id);
            if (product is null) return NotFound();

            var reviews = await _reviewService.GetReviewsByProductIdAsync(id);
            var averageRating = await _reviewService.GetAverageRatingForProductAsync(id);
            var reviewCount = await _reviewService.GetReviewCountForProductAsync(id);

            var viewModel = new ProductReviewViewModel
            {
                Product = product,
                Reviews = reviews.ToList(),
                AverageRating = averageRating,
                ReviewCount = reviewCount
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error fetching product reviews: {ex.Message}";
            return View("Error");
        }
    }
}
