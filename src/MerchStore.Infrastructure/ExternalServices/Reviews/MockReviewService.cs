using MerchStore.Domain.Entities;
using MerchStore.Domain.Enums;
using MerchStore.Domain.ValueObjects;

namespace MerchStore.Infrastructure.ExternalServices.Reviews;

/// <summary>
/// Provides mock review data for use as a fallback when the external API is unavailable
/// </summary>
public class MockReviewService
{
    // Slumpverktyg och exempeldata
    private static readonly Random _random = new Random();
    private static readonly string[] _customerNames = { "John Doe", "Jane Smith", "Bob Johnson", "Alice Brown", "Charlie Davis" };
    private static readonly string[] _reviewContents = {
        "I've been using this for weeks and it's fantastic.",
        "Exactly what I was looking for. High quality.",
        "The product is decent but shipping took too long.",
        "Works as advertised, very happy with my purchase.",
        "Good value for the money, would buy again."
    };

    /// <summary>
    /// Generates mock reviews for a product
    /// </summary>
    public (IEnumerable<Review> Reviews, ReviewStats Stats) GetProductReviews(Guid productId)
    {
        // Unik hash per produkt för att få olika mängd reviews
        var productIdHash = productId.GetHashCode();
        var reviewCount = Math.Abs(productIdHash % 6); // 0-5 reviews

        var reviews = GenerateMockReviews(productId, reviewCount);

        // Snittbetyg
        double averageRating = reviews.Any()
            ? Math.Round(reviews.Average(r => r.Rating), 1)
            : 0;

        var stats = new ReviewStats(productId, averageRating, reviewCount);

        return (reviews, stats);
    }

    // Genererar en lista med falska recensioner
    private IEnumerable<Review> GenerateMockReviews(Guid productId, int count)
    {
        var reviews = new List<Review>();
        var productSeed = productId.GetHashCode();
        var random = new Random(productSeed); // Samma produkt får samma recensioner

        for (int i = 0; i < count; i++)
        {
            int dayOffset = random.Next(1, 31);
            var createdAt = DateTime.UtcNow.AddDays(-dayOffset);

            // Realistisk betygsfördelning
            int ratingBase = random.Next(1, 101);
            int rating = ratingBase switch
            {
                <= 10 => 1,
                <= 25 => 2,
                <= 50 => 3,
                <= 80 => 4,
                _ => 5
            };

            string title = $"Sample Review: {i + 1} for Product";
            string customerName = _customerNames[random.Next(_customerNames.Length)];
            string content = _reviewContents[random.Next(_reviewContents.Length)];

            reviews.Add(new Review(
                Guid.NewGuid(),
                productId,
                customerName,
                title,
                content,
                rating,
                createdAt,
                ReviewStatus.Approved
            ));
        }

        return reviews.OrderByDescending(r => r.CreatedAt).ToList();
    }
}
