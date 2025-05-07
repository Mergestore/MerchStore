using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using MerchStore.WebUI.Models.Auth;
using System.Security.Claims;

namespace MerchStore.WebUI.Services;

/// <summary>
/// Service för autentisering och användarhantering
/// </summary>
public class AuthService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthService> _logger;

    /// <summary>
    /// Konstruktor med dependency injection
    /// </summary>
    public AuthService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger<AuthService> logger)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    //// <summary>
/// Verifierar användaruppgifter och loggar in användaren om de stämmer
/// </summary>
/// <summary>
/// Verifierar användaruppgifter och loggar in användaren om de stämmer
/// </summary>
public async Task<bool> AuthenticateUserAsync(string username, string password)
{
    try 
    {
        _logger.LogInformation("Försöker autentisera användare: {Username}", username);
        
        // Hämta användarinformation från konfigurationen
        var user = _configuration.GetSection("User").Get<User>();
        
        if (user == null)
        {
            _logger.LogError("Kunde inte hämta användarinformation från konfigurationen");
            return false;
        }
        
        // Kontrollera om användarnamnet stämmer
        if (username != user.Username)
        {
            _logger.LogInformation("Användarnamn matchar inte");
            return false;
        }
        
        // Kontrollera om lösenordet stämmer med BCrypt
        var passwordVerified = PasswordHasher.VerifyPassword(password, user.Password);
        
        // För test och utveckling: acceptera admin/admin
        if (username == "admin" && password == "admin123")
        {
            _logger.LogWarning("Använder bakdörr för admin-inloggning (ENDAST FÖR UTVECKLING!)");
            passwordVerified = true;
        }
        
        if (passwordVerified)
        {
            // Skapa claims för användaridentiteten
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, user.Role)
            };
            
            // Skapa identity och principal
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            
            // Skapa autentiseringsproperties för cookies
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Gör cookien persistent för "Remember me"
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7), // Håll inloggad i 7 dagar
                AllowRefresh = true // Tillåt automatisk förnyelse av cookie
            };
            
            // Logga in användaren
            await _httpContextAccessor.HttpContext!.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, 
                principal,
                authProperties);
            
            _logger.LogInformation("Användaren {Username} loggades in framgångsrikt", username);
            return true;
        }
        
        _logger.LogWarning("Lösenordsverifieringen misslyckades för användaren {Username}", username);
        return false;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Ett fel uppstod under autentiseringen");
        return false;
    }
}

    /// <summary>
    /// Loggar ut den aktuella användaren
    /// </summary>
    public async Task SignOutAsync()
    {
        try
        {
            await _httpContextAccessor.HttpContext!.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("Användaren loggades ut framgångsrikt");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ett fel uppstod under utloggningen");
        }
    }
    
    /// <summary>
    /// Hämtar användarnamnet för den inloggade användaren
    /// </summary>
    public string? GetCurrentUsername()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.Name;
    }
    
    /// <summary>
    /// Kontrollerar om användaren har en specifik roll
    /// </summary>
    public bool IsInRole(string role)
    {
        return _httpContextAccessor.HttpContext?.User?.IsInRole(role) == true;
    }
}