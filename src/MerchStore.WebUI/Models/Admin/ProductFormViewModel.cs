using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MerchStore.WebUI.Models.Admin;

public class ProductFormViewModel
{
    public Guid? Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Namn")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    [Display(Name = "Beskrivning")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Pris")]
    [Range(0.01, 1000000)]
    public decimal Price { get; set; }

    [Required]
    [Display(Name = "Valuta")]
    [StringLength(3)]
    public string Currency { get; set; } = "SEK";

    [Required]
    [Display(Name = "Lagersaldo")]
    [Range(1, 50000, ErrorMessage = "Lagersaldo måste vara mellan {1} och {2}.")]
    public int StockQuantity { get; set; }

    [Display(Name = "Produktbild")]
    public IFormFile? ImageFile { get; set; }

    public string? ExistingImageUrl { get; set; }
}