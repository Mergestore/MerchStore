using MerchStore.Domain.Common; // För att ärva från vår bas-Entity
using MerchStore.Domain.Enums;

namespace MerchStore.Domain.Entities;

/// <summary>
/// Representerar en recension som lämnats av en kund.
/// Det är en entitet (har ID och kan ändras över tid).
/// </summary>
public class Review : Entity<Guid>
{
    // 🆔 Koppling till produkt
    public Guid ProductId { get; private set; }

    // 🧑‍💬 Kundinfo och recensionstext
    public string CustomerName { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;

    // ⭐ Betyg (1-5 stjärnor)
    public int Rating { get; private set; }

    // 🕓 När recensionen lämnades
    public DateTime CreatedAt { get; private set; }

    // ✅⏳⛔ Status: Pending / Approved / Rejected
    public ReviewStatus Status { get; private set; }

    // 🛠 EF Core kräver parameterlös konstruktor (används internt)
    private Review() { }

    /// <summary>
    /// Skapa en ny recension med validering.
    /// </summary>
    public Review(
        Guid id,
        Guid productId,
        string customerName,
        string title,
        string content,
        int rating,
        DateTime createdAt,
        ReviewStatus status) : base(id)
    {
        // 🔐 Validering – affärsregler direkt i domänen
        if (productId == Guid.Empty)
            throw new ArgumentException("Produkt-ID får inte vara tomt.", nameof(productId));

        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Kundnamn får inte vara tomt.", nameof(customerName));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Titel får inte vara tom.", nameof(title));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Innehåll får inte vara tomt.", nameof(content));

        if (rating < 1 || rating > 5)
            throw new ArgumentOutOfRangeException(nameof(rating), "Betyget måste vara mellan 1 och 5.");

        // ✅ Sätt fält
        ProductId = productId;
        CustomerName = customerName;
        Title = title;
        Content = content;
        Rating = rating;
        CreatedAt = createdAt;
        Status = status;
    }
}
