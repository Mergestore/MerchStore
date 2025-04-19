namespace MerchStore.WebUI.Models.Catalog;


public class ShoppingCartItem
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
