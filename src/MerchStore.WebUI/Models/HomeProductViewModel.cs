using MerchStore.Domain.Entities;

namespace MerchStore.WebUI.Models;

/// <summary>
/// ViewModel för hemsidan som visar produktinformation + review-data
/// </summary>
public class HomeProductViewModel
{
    public Product Product { get; set; } = null!; // Själva produkten
    public double AverageRating { get; set; }     // Snittbetyg
    public int ReviewCount { get; set; }          // Antal recensioner
}
