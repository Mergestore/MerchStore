using Microsoft.AspNetCore.Identity;

namespace MerchStore.Infrastructure.Models.Auth;

/// <summary>
/// Representerar en användare i systemet.
/// Ärver från ASP.NET Core Identity - IdentityUser som innehåller egenskaper som:
/// - Id (string): Unikt GUID för användaren
/// - UserName (string): Användarnamn för inloggning
/// - Email (string): Användarens e-postadress
/// - PasswordHash (string): Hashat lösenord (aldrig i klartext)
/// - PhoneNumber (string): Telefonnummer om tillgängligt
/// 
/// Användarhantering i systemet:
/// 1. Användare kan registrera sig själva via Register-sidan
/// 2. Vid registrering tilldelas de automatiskt rollen "Customer"
/// 3. Admin-användare kan skapas manuellt eller via databas-seedning
/// 4. Det finns två roller i systemet:
///    - Admin: Har tillgång till admin-panelen och alla funktioner
///    - Customer: Standardroll för registrerade användare
/// 5. Autentisering hanteras av ASP.NET Core Identity med cookies
/// 6. UserManager<ApplicationUser> och SignInManager<ApplicationUser> används för att hantera användare
/// </summary>
public class ApplicationUser : IdentityUser
{
    // Egna porperties för användarens information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLogin { get; set; }
}
