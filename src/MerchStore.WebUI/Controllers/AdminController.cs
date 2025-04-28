using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MerchStore.WebUI.Controllers;

/// <summary>
/// Controller för administratörsfunktioner
/// Endast tillgänglig för användare med rollen Admin
/// </summary>
[Authorize(Policy = "AdminOnly")]
public class AdminController : Controller
{
    /// <summary>
    /// Visar administratörens Dashboard
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Visar en lista över alla produkter för administrering
    /// </summary>
    public IActionResult Products()
    {
        return View();
    }

    /// <summary>
    /// Visar en lista över användare för administrering
    /// </summary>
    public IActionResult Users()
    {
        return View();
    }
}