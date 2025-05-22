using MerchStore.Infrastructure.Models.Auth;
using MerchStore.WebUI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MerchStore.WebUI.Models.Auth;

namespace MerchStore.WebUI.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            // Försöker logga in användaren
            var result = await _signInManager.PasswordSignInAsync(
                model.Username,
                model.Password,
                isPersistent: true,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("Användare loggade in framgångsrikt.");
                return RedirectToLocal(returnUrl);
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Användaren är utlåst.");
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Ogiltigt inloggningsförsök.");
                return View(model);
            }
        }

        // Om vi kommer hit betyder det att något misslyckades, så visa formuläret igen
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("Användare loggade ut.");
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Lockout()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Tilldela användarrollen
                await _userManager.AddToRoleAsync(user, UserRoles.Customer);

                _logger.LogInformation("Användaren skapade ett nytt konto med lösenord.");

                // Logga in användaren direkt
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // Om vi kommer hit betyder det att något misslyckades, så visa formuläret igen
        return View(model);
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> AddAdminRole()
    {
        if (User.Identity != null && !User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Login");
        }

        if (User.Identity?.Name != null)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user != null)
            {
                // Kontrollera om användaren redan har rollen
                if (!await _userManager.IsInRoleAsync(user, Domain.Constants.UserRoles.Administrator))
                {
                    // Lägg till Admin-rollen
                    await _userManager.AddToRoleAsync(user, Domain.Constants.UserRoles.Administrator);
                    TempData["SuccessMessage"] = "Admin-rollen har lagts till till din användare. Logga ut och in igen för att aktivera den.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Du har redan Admin-rollen.";
                }
            }
        }

        return RedirectToAction("Login");
    }
}