using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MerchStore.Infrastructure.ExternalServices.Reviews.Models;
using System.Text.Json;

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

    /*// Hämtar recensioner för en produkt från API:et
    public async Task<ReviewResponseDto?> GetProductReviewsAsync(Guid productId)
    {
        try
        {
            string url = $"products/{productId}/reviews";

            _logger.LogInformation("Requesting reviews for product {ProductId} from external API", productId);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode(); // Kastar exception om status är 400+

            var reviewsResponse = await response.Content.ReadFromJsonAsync<ReviewResponseDto>();

            _logger.LogInformation("Successfully retrieved {ReviewCount} reviews for product {ProductId}",
                reviewsResponse?.Reviews?.Count ?? 0, productId);

            // Logga JSON-respons snyggt i debug-läge
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                var json = JsonSerializer.Serialize(reviewsResponse, _prettyJsonOptions);
                _logger.LogDebug("Received response: {Json}", json);
            }

            return reviewsResponse;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while fetching reviews for product {ProductId}: {Message}",
                productId, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching reviews for product {ProductId}: {Message}",
                productId, ex.Message);
            throw;
        }
    }*/
     // Hämtar recensioner för en produkt från API:et, nu med "group" parameter istället för "productId"
    public async Task<ReviewResponseDto?> GetGroupReviewsAsync(Guid productId)
    {
        try
        {
            // Konvertera GUID till en lämplig gruppbeteckning som API:et förstår
            // Exempel: konvertera en GUID till en produktkategori eller produktnummer
            string groupIdentifier = ConvertProductIdToGroupIdentifier(productId);
            
            // Bygg URL med den nya "group" query-parametern
            string url = $"api/v1/group-reviews?group={groupIdentifier}";

            _logger.LogInformation("Requesting reviews for group {GroupId} from external API (originally product ID: {ProductId})", 
                groupIdentifier, productId);

            var response = await _httpClient.GetAsync(url);
            
            // Om API:et returnerar 404, kan vi hantera det särskilt
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Group {GroupId} not found in external API", groupIdentifier);
                return new ReviewResponseDto
                {
                    Reviews = new List<ReviewDto>(),
                    Stats = new ReviewStatsDto 
                    { 
                        GroupId = groupIdentifier,
                        AverageRating = 0,
                        ReviewCount = 0
                    }
                };
            }
            
            // För andra fel, kasta exception
            response.EnsureSuccessStatusCode();

            var reviewsResponse = await response.Content.ReadFromJsonAsync<ReviewResponseDto>();

            _logger.LogInformation("Successfully retrieved {ReviewCount} reviews for group {GroupId}",
                reviewsResponse?.Reviews?.Count ?? 0, groupIdentifier);

            // Logga JSON-respons snyggt i debug-läge
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                var json = JsonSerializer.Serialize(reviewsResponse, _prettyJsonOptions);
                _logger.LogDebug("Received response: {Json}", json);
            }

            return reviewsResponse;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while fetching reviews for product {ProductId}: {StatusCode} {Message}",
                productId, ex.StatusCode, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching reviews for product {ProductId}: {Message}",
                productId, ex.Message);
            throw;
        }
    }
    
    // Hjälpmetod för att konvertera produkt-ID till en gruppreferens
    private string ConvertProductIdToGroupIdentifier(Guid productId)
    {
        // När vi inte vet exakt vilka gruppvärden som API:t accepterar kan vi prova några enkla strategier
        
        // Strategi 1: Använd en enkel hashfunktion baserad på produktID
        // I produktion skulle detta idealt vara en riktig mappning till godkända grupper
        var productHash = Math.Abs(productId.GetHashCode());
        
        // Skapa några enkla grupper baserat på hash-värdet
        string[] possibleGroups = { "clothing", "accessories", "equipment", "nutrition", "footwear" };
        int groupIndex = productHash % possibleGroups.Length;
        
        return possibleGroups[groupIndex];
    }
}
