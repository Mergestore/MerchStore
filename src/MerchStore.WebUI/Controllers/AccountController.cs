using System.Security.Claims;
using MerchStore.WebUI.Models;
using MerchStore.WebUI.Models.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MerchStore.Controllers;

public class AccountController : Controller
{
    // Simulated user database - in production, this would be in a database
    private static readonly Dictionary<string, (string PasswordHash, string Role)> Users = new()
    {
        // Password: "admin123" (hashed with BCrypt)
        ["admin"] = ("$2a$12$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy", UserRoles.Administrator),
        // Password: "password123" (hashed with BCrypt)
        ["john.doe"] = ("$2a$12$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy", UserRoles.Customer)
    };

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginAsync(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // För utveckling
        if ((model.Username == "admin" && model.Password == "admin123") ||
            (model.Username == "john.doe" && model.Password == "password123"))
        {
            var role = model.Username == "admin" ? UserRoles.Administrator : UserRoles.Customer;

            // Create claims including role
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.Username!),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Sign in
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            // Redirect to return URL if valid, otherwise to home
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    // Utility method to hash passwords (for demonstration)
    [HttpGet]
    [Authorize(Roles = UserRoles.Administrator)]
    public IActionResult HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return BadRequest("Password is required");
        }

        var hash = BCrypt.Net.BCrypt.HashPassword(password, 12); // Using work factor 12 for security
        return Ok(new { password, hash });
    }
}