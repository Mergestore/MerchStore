using MerchStore.Domain.Entities;

namespace MerchStore.WebUI.Models;

/// <summary>
/// ViewModel for a single product and its reviews.
/// </summary>
public class ProductReviewViewModel
{
    public Product Product { get; set; } = null!;
    public List<Review> Reviews { get; set; } = new();
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
}
