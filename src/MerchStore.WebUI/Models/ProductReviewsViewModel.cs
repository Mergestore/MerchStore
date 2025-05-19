using MerchStore.Domain.Entities;

namespace MerchStore.WebUI.Models;

/// <summary>
/// ViewModel for displaying a list of products with review data.
/// </summary>
public class ProductReviewsViewModel
{
    public List<Product> Products { get; set; } = new();
    public Dictionary<Guid, IEnumerable<Review>> ProductReviews { get; set; } = new();
    public Dictionary<Guid, double> AverageRatings { get; set; } = new();
    public Dictionary<Guid, int> ReviewCounts { get; set; } = new();
}
