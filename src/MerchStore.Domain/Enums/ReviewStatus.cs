namespace MerchStore.Domain.Enums;

/// <summary>
/// Enum som representerar status för en recension.
/// Pending = väntar på godkännande,
/// Approved = godkänd och synlig,
/// Rejected = avvisad.
/// </summary>
public enum ReviewStatus
{
    Pending,
    Approved,
    Rejected
}
