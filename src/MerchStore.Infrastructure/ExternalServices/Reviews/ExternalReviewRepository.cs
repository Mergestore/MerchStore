// src/MerchStore.Infrastructure/ExternalServices/Reviews/ExternalReviewRepository.cs
using MerchStore.Domain.Entities;
using MerchStore.Domain.Enums;
using MerchStore.Domain.Interfaces;
using MerchStore.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;

namespace MerchStore.Infrastructure.ExternalServices.Reviews;

/// <summary>
/// Repository implementation that integrates with the external review API
/// and implements circuit breaker pattern for resilience
/// </summary>
public class ExternalReviewRepository : IReviewRepository
{
    private readonly ReviewApiClient _apiClient;
    private readonly MockReviewService _mockReviewService;
    private readonly ILogger<ExternalReviewRepository> _logger;
    private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

    public ExternalReviewRepository(
        ReviewApiClient apiClient,
        MockReviewService mockReviewService,
        IOptions<ReviewApiOptions> options,
        ILogger<ExternalReviewRepository> logger)
    {
        _apiClient = apiClient;
        _mockReviewService = mockReviewService;
        _logger = logger;

        var circuitOptions = options.Value;

        // 🛡️ Setup Polly Circuit Breaker: Stoppa anrop efter X misslyckanden
        _circuitBreakerPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: circuitOptions.ExceptionsAllowedBeforeBreaking,
                durationOfBreak: TimeSpan.FromSeconds(circuitOptions.CircuitBreakerDurationSeconds),
                onBreak: (ex, breakDuration) =>
                {
                    _logger.LogWarning(
                        "⛔ Circuit breaker öppnade i {BreakDuration} sek pga {ExceptionType}: {ExceptionMessage}",
                        breakDuration, ex.GetType().Name, ex.Message);
                },
                onReset: () =>
                {
                    _logger.LogInformation("✅ Circuit breaker återställd – anrop återupptas");
                },
                onHalfOpen: () =>
                {
                    _logger.LogInformation("🤞 Circuit half-open – testar API:t");
                });
    }

    public async Task<(IEnumerable<Review> Reviews, ReviewStats Stats)> GetProductReviewsAsync(Guid productId, int limit = 10, int offset = 0)
    {
        try
        {
            // 🧠 Kör anropet genom circuit breaker-skyddet
            return await _circuitBreakerPolicy.ExecuteAsync(async () =>
            {
                // Anropa den nya metoden för grupprecensioner
                var response = await _apiClient.GetGroupReviewsAsync(productId);

                // Kontrollera om vi fått komplett data
                if (response?.Reviews == null || response.Stats == null)
                {
                    throw new InvalidOperationException("External API returned incomplete data");
                }

                // Mappa från DTO till riktiga domänobjekt
                var reviews = response.Reviews.Select(r => new Review(
                    Guid.Parse(r.Id ?? Guid.NewGuid().ToString()),
                    productId, // Vi använder det ursprungliga produkt-ID:t här, inte grupp-ID:t
                    r.CustomerName ?? "Unknown",
                    r.Title ?? "No Title",
                    r.Content ?? "No Content",
                    r.Rating,
                    r.CreatedAt,
                    ParseReviewStatus(r.Status)
                )).ToList();

                var stats = new ReviewStats(
                    productId, // Ursprungligt produkt-ID
                    response.Stats.AverageRating,
                    response.Stats.ReviewCount
                );

                return (reviews, stats);
            });
        }
        catch (BrokenCircuitException)
        {
            // 🚨 Om kretsen är öppen → använd fallback/mock
            _logger.LogWarning("⚠️ Circuit är öppen – använder mock data för produkt {ProductId}", productId);
            return _mockReviewService.GetProductReviews(productId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Fel vid hämtning av recensioner för produkt {ProductId} – använder mock", productId);
            return _mockReviewService.GetProductReviews(productId);
        }
    }

    // 🔁 Översätt textstatus till enum
    private static ReviewStatus ParseReviewStatus(string? status)
    {
        return status?.ToLowerInvariant() switch
        {
            "approved" => ReviewStatus.Approved,
            "rejected" => ReviewStatus.Rejected,
            "pending" => ReviewStatus.Pending,
            _ => ReviewStatus.Pending
        };
    }

}