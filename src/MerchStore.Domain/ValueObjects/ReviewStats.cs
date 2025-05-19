namespace MerchStore.Domain.ValueObjects;

/// <summary>
/// Representerar statistik för recensioner på en produkt.
/// Detta är ett Value Object – det är oföränderligt (immutable).
/// </summary>
public record ReviewStats
{
    public Guid ProductId { get; }
    public double AverageRating { get; }
    public int ReviewCount { get; }

    // Konstruktorn säkerställer att datan alltid är giltig.
    public ReviewStats(Guid productId, double averageRating, int reviewCount)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID kan inte vara tomt", nameof(productId));

        if (averageRating < 0 || averageRating > 5)
            throw new ArgumentOutOfRangeException(nameof(averageRating), "Snittbetyget måste vara mellan 0 och 5");

        if (reviewCount < 0)
            throw new ArgumentOutOfRangeException(nameof(reviewCount), "Antal recensioner får inte vara negativt");

        ProductId = productId;
        AverageRating = averageRating;
        ReviewCount = reviewCount;
    }
}
