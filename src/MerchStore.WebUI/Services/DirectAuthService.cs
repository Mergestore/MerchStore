using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace MerchStore.WebUI.Services;

/// <summary>
/// En extremt förenklad autentiseringstjänst för utveckling och testning.
/// ANVÄND INTE I PRODUKTION!
/// </summary>
public class DirectAuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<DirectAuthService> _logger;

    public DirectAuthService(IHttpContextAccessor httpContextAccessor, ILogger<DirectAuthService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Autentiserar en användare direkt med fasta värden (endast för utveckling)
    /// </summary>
    public async Task<bool> AuthenticateUserAsync(string username, string password)
    {
        _logger.LogWarning("Använder DirectAuthService - ENDAST FÖR UTVECKLING!");

        // Extremt förenklad autentisering för utveckling
        if (username == "admin" && password == "admin")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await _httpContextAccessor.HttpContext!.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            _logger.LogInformation("Admin-användare loggades in via direktautentisering");
            return true;
        }

        _logger.LogInformation("Direktautentisering misslyckades för användare: {Username}", username);
        return false;
    }

    /// <summary>
    /// Loggar ut användaren
    /// </summary>
    public async Task SignOutAsync()
    {
        await _httpContextAccessor.HttpContext!.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);
    }
}