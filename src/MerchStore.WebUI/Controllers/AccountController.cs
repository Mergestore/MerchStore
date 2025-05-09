using System.Security.Claims;
using MerchStore.WebUI.Models;
using MerchStore.WebUI.Models.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MerchStore.WebUI.Services;

namespace MerchStore.Controllers;

/// <summary>
/// Hanterar användarinloggning, utloggning och åtkomstkontroll
/// </summary>
public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;
    private readonly AuthService _authService;

    public AccountController(
        ILogger<AccountController> logger,
        AuthService authService)
    {
        _logger = logger;
        _authService = authService;
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

        if (await _authService.AuthenticateUserAsync(model.Username!, model.Password!))
        {
            _logger.LogInformation("Användare loggade in framgångsrikt.");
            return RedirectToLocal(returnUrl);
        }

        ModelState.AddModelError(string.Empty, "Ogiltigt användarnamn eller lösenord.");
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

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("Index", "Home");
    }
}