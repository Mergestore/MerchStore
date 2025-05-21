namespace MerchStore.Infrastructure.ExternalServices.Reviews.Models
{
    public class ReviewDto
    {
        public string? Id { get; set; }
        public string? GroupId { get; set; }
        public string? CustomerName { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Status { get; set; }
    }
}