namespace MerchStore.Infrastructure.ExternalServices.Reviews.Configurations;

public class ReviewApiOptions
{
    public const string SectionName = "ReviewApi";

    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiKeyHeaderName { get; set; } = "x-functions-key";
    public int TimeoutSeconds { get; set; } = 30;

    // Circuit breaker settings
    public int ExceptionsAllowedBeforeBreaking { get; set; } = 3;
    public int CircuitBreakerDurationSeconds { get; set; } = 30;
}