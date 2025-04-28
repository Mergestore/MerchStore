using System.ComponentModel.DataAnnotations;

namespace MerchStore.WebUI.Models.Auth;

/// <summary>
/// ViewModel för inloggningsformuläret
/// </summary>
public class LoginViewModel
{
    /// <summary>
    /// Användarnamnet för inloggning
    /// </summary>
    [Required(ErrorMessage = "Användarnamn måste anges")]
    [Display(Name = "Användarnamn")]
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Lösenordet för inloggning
    /// </summary>
    [Required(ErrorMessage = "Lösenord måste anges")]
    [Display(Name = "Lösenord")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// URL att återgå till efter inloggning
    /// </summary>
    public string ReturnUrl { get; set; } = string.Empty;
}