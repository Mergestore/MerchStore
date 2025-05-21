namespace MerchStore.WebUI.Models.Catalog;

public class ProductCatalogViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PriceAmount { get; set; }
    public int StockQuantity { get; set; }

    public List<ProductCardViewModel> FeaturedProducts { get; set; } = new List<ProductCardViewModel>();
    
}