namespace MerchStore.Infrastructure.ExternalServices.Reviews.Models
{
    public class ReviewDto
    {
        public string? Id { get; set; }
        public string? GroupId { get; set; } // Ändrad från ProductId till GroupId för att matcha nya API:et
        public string? CustomerName { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Status { get; set; }
    }
}