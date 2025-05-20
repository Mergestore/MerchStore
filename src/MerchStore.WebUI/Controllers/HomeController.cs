using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MerchStore.WebUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MerchStore.Infrastructure.Models.Auth;


namespace MerchStore.WebUI.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }

    [Authorize] // This attribute ensures that only authenticated users can access this action.
    public IActionResult WhoAmI()
    {
        var viewModel = new
        {
            IsAuthenticated = User.Identity?.IsAuthenticated,
            AuthenticationType = User.Identity?.AuthenticationType,
            Name = User.Identity?.Name,
            Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
        };

        return View(viewModel);
    }

    // Enkel åtgärd för att visa användarens rollinfo
    [Authorize]
    public IActionResult ShowRoles()
    {
        var roles = User.Claims
            .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        ViewBag.Roles = roles;
        ViewBag.UserName = User.Identity?.Name;
        ViewBag.IsInAdminRole = User.IsInRole(MerchStore.Domain.Constants.UserRoles.Administrator);

        return View();
    }
}
