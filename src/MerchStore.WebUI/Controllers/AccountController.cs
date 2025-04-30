using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MerchStore.WebUI.Models.Auth;
using MerchStore.WebUI.Services;

namespace MerchStore.WebUI.Controllers;

/// <summary>
/// Controller för att hantera användarinloggning och utloggning
/// </summary>
public class AccountController : Controller
{
    private readonly AuthService _authService;
    private readonly ILogger<AccountController> _logger;

    /// <summary>
    /// Konstruktor med dependency injection
    /// </summary>
    public AccountController(AuthService authService, ILogger<AccountController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Visar inloggningssidan
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        _logger.LogInformation("Visar inloggningssidan. ReturnUrl: {ReturnUrl}", returnUrl);
        
        // Visa felmeddelande om det finns i TempData
        if (TempData["ErrorMessage"] != null)
        {
            ModelState.AddModelError(string.Empty, TempData["ErrorMessage"]!.ToString()!);
        }

        // Sätt returnUrl till hemskärmen om den inte är specificerad
        returnUrl ??= Url.Content("~/");

        // Skapa och returnera vy-modellen
        var viewModel = new LoginViewModel
        {
            ReturnUrl = returnUrl
        };

        return View(viewModel);
    }

    /// <summary>
    /// Hanterar inloggningsförsök
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        _logger.LogInformation("Inloggningsförsök för användare: {Username}", model.Username);
        
        // Standardisera returnUrl
        model.ReturnUrl ??= Url.Content("~/");

        if (ModelState.IsValid)
        {
            // Försök autentisera användaren
            var isAuthenticated = await _authService.AuthenticateUserAsync(model.Username, model.Password);

            if (isAuthenticated)
            {
                _logger.LogInformation("Användare {Username} loggades in framgångsrikt", model.Username);
                // Lyckad inloggning, omdirigera till returnUrl
                return LocalRedirect(model.ReturnUrl);
            }

            _logger.LogWarning("Misslyckad inloggning för användare: {Username}", model.Username);
            // Felaktig inloggning, visa felmeddelande
            ModelState.AddModelError(string.Empty, "Felaktigt användarnamn eller lösenord.");
        }
        else
        {
            _logger.LogInformation("Validering av inloggningsformulär misslyckades");
        }

        // Om vi kommer hit har något gått fel, visa formuläret igen
        return View(model);
    }

    /// <summary>
    /// Loggar ut användaren
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        _logger.LogInformation("Loggar ut användare: {Username}", User.Identity?.Name);
        
        // Logga ut användaren
        await _authService.SignOutAsync();
        
        // Omdirigera till hemsidan
        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Visar sidan för nekad åtkomst
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        _logger.LogWarning("Åtkomst nekad för användare: {Username}", User.Identity?.Name);
        return View();
    }
}