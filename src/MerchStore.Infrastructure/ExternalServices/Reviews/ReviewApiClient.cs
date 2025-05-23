using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MerchStore.Infrastructure.ExternalServices.Reviews.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MerchStore.Infrastructure.ExternalServices.Reviews;

public class ReviewApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ReviewApiClient> _logger;
    private readonly ReviewApiOptions _options;

    // JSON-inställningar för att skriva ut snygg JSON i loggar
    private static readonly JsonSerializerOptions _prettyJsonOptions = new() { WriteIndented = true };

    // Konstruktor – tar in HttpClient, konfiguration och loggning
    public ReviewApiClient(
        HttpClient httpClient,
        IOptions<ReviewApiOptions> options,
        ILogger<ReviewApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;

        // Konfigurera HttpClient baserat på ReviewApiOptions
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);


        // Lägg till API-nyckel i header
        _httpClient.DefaultRequestHeaders.Add(_options.ApiKeyHeaderName, _options.ApiKey);

        // Lägg även till Bearer token om det behövs
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");

        // Sätt timeout
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
    }

    // Hämtar recensioner för en produkt från API:et, nu med "group" parameter istället för "productId"
    public async Task<ReviewResponseDto?> GetGroupReviewsAsync(Guid productId)
    {
        try
        {
            // Använd alltid "group4" som grupp
            string groupIdentifier = "group4";
            string url = $"api/v1/group-reviews?group={groupIdentifier}";

            _logger.LogInformation("Requesting reviews for group {GroupId}", groupIdentifier);

            var response = await _httpClient.GetAsync(url);

            // Hantera fel
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("API-anrop misslyckades: {StatusCode}", response.StatusCode);
                return CreateEmptyResponse(productId);
            }

            // Läs svaret från API:et
            string responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Rådata från API: {Length} bytes", responseContent.Length);

            // Tomt svar?
            if (string.IsNullOrWhiteSpace(responseContent) || responseContent == "[]")
            {
                _logger.LogInformation("API returned empty array");
                return CreateEmptyResponse(productId);
            }

            // Deserialisera till den nya modellen
            var productReviews = await response.Content.ReadFromJsonAsync<List<ProductReviewsDto>>();

            if (productReviews == null || !productReviews.Any())
            {
                _logger.LogWarning("API returned no product reviews");
                return CreateEmptyResponse(productId);
            }

            // Hitta den aktuella produkten med samma ID (om den finns)
            var productReview = productReviews.FirstOrDefault(p =>
                p.ProductId != null && Guid.TryParse(p.ProductId, out var id) && id == productId);

            // Om produkten inte hittades, returnera tomt svar
            if (productReview == null || productReview.Reviews == null || productReview.Reviews.Reviews == null)
            {
                _logger.LogInformation("Product {ProductId} not found in API response", productId);
                return CreateEmptyResponse(productId);
            }

            // Extrahera betygsdata
            string totalReviewsStr = productReview.Reviews.TotalReviews ?? "0 st";
            int reviewCount = int.TryParse(totalReviewsStr.Split(' ')[0], out var count) ? count : 0;

            // Extrahera rating från formatterad sträng (t.ex. "★★★★½ (4.5 av 5)")
            string formattedRating = productReview.Reviews.FormattedRating ?? "★★★☆☆ (0.0 av 5)";
            double rating = 0.0;

            // Försök hitta värdet inom parentes (t.ex. "4.5 av 5")
            int startIndex = formattedRating.IndexOf('(');
            int endIndex = formattedRating.IndexOf(' ', startIndex);
            if (startIndex >= 0 && endIndex > startIndex)
            {
                string ratingStr = formattedRating.Substring(startIndex + 1, endIndex - startIndex - 1);
                double.TryParse(ratingStr, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out rating);
            }

            // Konvertera API-modellen till vår domänmodell
            var reviewDtos = productReview.Reviews.Reviews.Select(r => new ReviewDto
            {
                Id = Guid.NewGuid().ToString(), // Generera ett ID eftersom det inte finns i svaret
                GroupId = productReview.ProductId,
                CustomerName = "Kund", // Standardvärde eftersom det inte finns i svaret
                Title = "Recension", // Standardvärde eftersom det inte finns i svaret
                Content = r.ReviewContent ?? "Ingen recension",
                Rating = (int)Math.Round(rating), // Använd genomsnittsbetyget för alla recensioner
                CreatedAt = DateTime.UtcNow.AddDays(-new Random().Next(1, 30)), // Slumpmässigt datum
                Status = "approved"
            }).ToList();

            // Skapa och returnera vår responsmodell
            var reviewResponse = new ReviewResponseDto
            {
                Reviews = reviewDtos,
                Stats = new ReviewStatsDto
                {
                    GroupId = productReview.ProductId,
                    AverageRating = rating,
                    ReviewCount = reviewCount
                }
            };

            _logger.LogInformation("Successfully mapped {ReviewCount} reviews for product {ProductId}",
                reviewDtos.Count, productId);

            return reviewResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching reviews for product {ProductId}", productId);
            return CreateEmptyResponse(productId);
        }
    }

// Hjälpmetod för att skapa ett tomt svar
    private ReviewResponseDto CreateEmptyResponse(Guid productId)
    {
        return new ReviewResponseDto
        {
            Reviews = new List<ReviewDto>(),
            Stats = new ReviewStatsDto
            {
                GroupId = productId.ToString(),
                AverageRating = 0,
                ReviewCount = 0
            }
        };

    }
}