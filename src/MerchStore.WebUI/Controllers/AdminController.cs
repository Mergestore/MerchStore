using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MerchStore.Domain.Constants;

namespace MerchStore.WebUI.Controllers;

[Authorize(Roles = UserRoles.Administrator)]
public class AdminController : Controller
{
    public IActionResult Dashboard()
    {
        return View();
    }

    public IActionResult Users()
    {
        // In a real application, this would fetch from a database
        var users = new List<string> { "admin", "john.doe" };
        return View(users);
    }

    public IActionResult Orders()
    {
        // Placeholder for order management
        return View();
    }

    public IActionResult Statistics()
    {
        // Placeholder for statistics dashboard
        return View();
    }
}