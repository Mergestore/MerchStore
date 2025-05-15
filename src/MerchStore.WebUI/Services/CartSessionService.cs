using MerchStore.WebUI.Models;
using System.Text.Json;
using MerchStore.WebUI.Models.Catalog;
using System.Globalization;

namespace MerchStore.WebUI.Services;

public class CartSessionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string SessionKey = "ShoppingCart";

    public CartSessionService(IHttpContextAccessor accessor)
    {
        _httpContextAccessor = accessor;
    }

    public List<ShoppingCartItem> GetCart()
    {
        var session = _httpContextAccessor.HttpContext?.Session; // Get the current session
        var cartJson = session?.GetString(SessionKey); // Retrieve the cart JSON string from the session
        return cartJson != null
            ? JsonSerializer.Deserialize<List<ShoppingCartItem>>(cartJson) ?? new()
            : new List<ShoppingCartItem>();
    }


    public void AddToCart(ShoppingCartItem item)
    {
        var cart = GetCart();
        var existing = cart.FirstOrDefault(p => p.ProductId == item.ProductId);

        if (existing != null)
        {
            existing.Quantity += item.Quantity;
        }
        else
        {
            cart.Add(item);
        }

        SaveCart(cart);
    }
    
    public void SaveCart(List<ShoppingCartItem> cart)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        session?.SetString(SessionKey, JsonSerializer.Serialize(cart));
    }

    public void RemoveFromCart(Guid productId)
    {
        var cart = GetCart();
        cart.RemoveAll(p => p.ProductId == productId);
        SaveCart(cart);
    }

    public void ClearCart()
    {
        SaveCart(new List<ShoppingCartItem>());
    }

    public void UpdateQuantity(Guid productId, int newQuantity)
    {
        var cart = GetCart();
        var item = cart.FirstOrDefault(i => i.ProductId == productId);
        if (item != null && newQuantity > 0)
        {
            item.Quantity = newQuantity;
            _httpContextAccessor.HttpContext?.Session.SetString(SessionKey, JsonSerializer.Serialize(cart));
        }
    }

}
