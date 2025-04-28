using System.ComponentModel.DataAnnotations;

namespace MerchStore.WebUI.Models.Auth;

/// <summary>
/// Representerar en användare i systemet med autentiseringsinformation
/// </summary>
public class User
{
    /// <summary>
    /// Användarnamnet för inloggning
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Lösenordet i hashad form (aldrig i klartext!)
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// Salt som används vid lösenordshashing för ökad säkerhet
    /// </summary>
    public string Salt { get; set; } = string.Empty;
    
    /// <summary>
    /// Användarens roll i systemet (t.ex. Customer eller Admin)
    /// </summary>
    public string Role { get; set; } = "Customer";
}