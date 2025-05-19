namespace MerchStore.Infrastructure.ExternalServices.Reviews.Models;

public class ReviewStatsDto
{
    public string? ProductId { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
}