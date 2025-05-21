namespace MerchStore.Infrastructure.ExternalServices.Reviews.Models
{
    public class ProductReviewsDto
    {
        public string? ProductId { get; set; }
        public string? ProductName { get; set; }
        public ProductReviewsDetailDto? Reviews { get; set; }
    }
}