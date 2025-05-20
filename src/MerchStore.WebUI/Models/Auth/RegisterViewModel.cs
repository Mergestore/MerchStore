using System.ComponentModel.DataAnnotations;

namespace MerchStore.WebUI.Models.Auth;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Användarnamn måste anges.")]
    [Display(Name = "Användarnamn")]
    public string UserName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Förnamn måste anges.")]
    [Display(Name = "Förnamn")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Efternamn måste anges.")]
    [Display(Name = "Efternamn")]
    public string LastName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "E-post måste anges.")]
    [EmailAddress(ErrorMessage = "Ogiltig e-postadress.")]
    [Display(Name = "E-post")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Lösenord måste anges.")]
    [StringLength(100, ErrorMessage = "Lösenordet måste vara minst {2} tecken långt.", MinimumLength = 4)]
    [Display(Name = "Lösenord")]
    public string Password { get; set; } = string.Empty;
    
}