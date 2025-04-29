using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MerchStore.WebUI.Models;
using MerchStore.WebUI.Services;
using System; 
using MerchStore.WebUI.Models.Catalog;


namespace MerchStore.WebUI.Controllers
{
    // Kräver att användaren är inloggad för alla actions i denna controller
    //[Authorize] lade en kommentar på denna för att kund ej ska behöva logga in för att kunna lägga till i kundvagnen
    public class CartController : Controller
    {
        private readonly CartSessionService _cartService;

        // Konstruktor för att hämta in CartSessionService via Dependency Injection
        public CartController(CartSessionService cartService)
        {
            _cartService = cartService;
        }

        // Visar innehållet i kundvagnen
        public IActionResult Index()
        {
            var cart = _cartService.GetCart();
            return View(cart); // Detta ska visa Cart/Index.cshtml
        }

        // Lägger till en produkt i kundvagnen (skickas från "Lägg till" knapp)
        [HttpPost]
        public IActionResult AddToCart(Guid id, string name, decimal price, int quantity)
        {
            var item = new ShoppingCartItem
            {
                ProductId = id,
                Name = name,
                Price = price,
                Quantity = quantity
            };

            _cartService.AddToCart(item);


            TempData["SuccessMessage"] = $"{quantity} st av \"{name}\" lades till i kundvagnen.";
            // 🛒 Gå tillbaka till sidan användaren var på
            return Redirect(Request.Headers["Referer"].ToString());


            // Efter man lagt till så går man till kundvagnen
            //return RedirectToAction("Index");
        }

        // Tar bort en produkt från kundvagnen baserat på dess ID
        [HttpPost]
        public IActionResult Remove(Guid id)
        {
            _cartService.RemoveFromCart(id);
            return RedirectToAction("Index");
        }
    }
}