// src/MerchStore.Infrastructure/ExternalServices/Reviews/Configurations/ReviewApiOptions.cs

public class ReviewApiOptions
{
    public const string SectionName = "ReviewApi";

    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiKeyHeaderName { get; set; } = "X-API-KEY";
    public int TimeoutSeconds { get; set; } = 30;

    // Circuit breaker settings
    public int ExceptionsAllowedBeforeBreaking { get; set; } = 3;
    public int CircuitBreakerDurationSeconds { get; set; } = 30;
}