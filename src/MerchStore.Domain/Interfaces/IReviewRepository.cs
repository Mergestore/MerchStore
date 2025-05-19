using MerchStore.Domain.Entities;
using MerchStore.Domain.ValueObjects;

namespace MerchStore.Domain.Interfaces;

/// <summary>
/// Interface för att hämta recensioner och statistik om en produkt.
/// Här beskriver vi vad vi vill kunna göra – inte hur.
/// </summary>
public interface IReviewRepository
{
    /// <summary>
    /// Hämtar alla recensioner för en viss produkt + statistik i ett enda anrop.
    /// </summary>
    /// <param name="productId">ID för produkten</param>
    /// <returns>
    /// En tuple med alla recensioner samt ett ReviewStats-objekt med snittbetyg och antal.
    /// </returns>
    Task<(IEnumerable<Review> Reviews, ReviewStats Stats)> GetProductReviewsAsync(Guid productId);
}
