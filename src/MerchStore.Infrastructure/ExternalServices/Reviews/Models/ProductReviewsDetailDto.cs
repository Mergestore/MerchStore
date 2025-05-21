namespace MerchStore.Infrastructure.ExternalServices.Reviews.Models
{
    public class ProductReviewsDetailDto
    {
        public string? FormattedRating { get; set; }
        public string? TotalReviews { get; set; }
        public List<ReviewContentDto>? Reviews { get; set; }
    }
}