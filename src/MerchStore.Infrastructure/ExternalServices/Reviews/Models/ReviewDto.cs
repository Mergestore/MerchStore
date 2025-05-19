namespace MerchStore.Infrastructure.ExternalServices.Reviews.Models;

public class ReviewDto
{
    public string? Id { get; set; }
    public string? ProductId { get; set; }
    public string? CustomerName { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Status { get; set; }
}