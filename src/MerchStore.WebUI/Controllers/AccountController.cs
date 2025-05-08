using System.Security.Claims;
using MerchStore.WebUI.Models;
using MerchStore.WebUI.Models.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MerchStore.Controllers;

/// <summary>
/// Hanterar användarinloggning, utloggning och åtkomstkontroll
/// </summary>
public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;
    private readonly IConfiguration _configuration;

    public AccountController(
        ILogger<AccountController> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    // Visar inloggningssidan
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // Hanterar inloggningsförsök
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginAsync(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (IsValidCookieAuthUser(model.Username, model.Password))
        {
            var role = model.Username == "admin" ? UserRoles.Administrator : UserRoles.Customer;
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.Username!),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            _logger.LogInformation("Användare loggade in med cookie-autentisering.");
            return RedirectToLocal(returnUrl);
        }

        ModelState.AddModelError(string.Empty, "Ogiltigt inloggningsförsök.");
        return View(model);
    }

    // Hanterar utloggning
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _logger.LogInformation("Användare loggade ut.");
        return RedirectToAction("Index", "Home");
    }

    // Visar åtkomstnekad-sidan
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    // Validerar användarnamn och lösenord (endast för utveckling)
    private bool IsValidCookieAuthUser(string? username, string? password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            return false;

        // För utveckling
        return (username == "admin" && password == "admin123") ||
               (username == "john.doe" && password == "password123");
    }

    // Omdirigerar till returnUrl om den är lokal, annars till startsidan
    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("Index", "Home");
    }
}