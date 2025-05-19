namespace MerchStore.Infrastructure.ExternalServices.Reviews.Models;

public class ReviewResponseDto
{
    public List<ReviewDto>? Reviews { get; set; }
    public ReviewStatsDto? Stats { get; set; }
}