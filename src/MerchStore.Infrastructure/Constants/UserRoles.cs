namespace MerchStore.Domain.Constants;

/// <summary>
/// Definierar användarroller i applikationen.
/// 
/// Rollbaserad behörighetskontroll (RBAC):
/// ------------------------------------------
/// 1. Roller i systemet:
///    - Administrator: Har tillgång till admin-panel och alla funktioner
///    - Customer: Standardroll för registrerade användare
/// 
/// 2. Behörighetshantering:
///    - Roller tilldelas vid skapande av användare
///    - Admin-åtkomst kontrolleras med [Authorize(Roles = UserRoles.Administrator)]-attributet
///    - Användarens roller lagras i ASP.NET Identity-systemet (AspNetUserRoles-tabellen)
/// 
/// 3. Rollhantering i koden:
///    - Tilldela roller: await _userManager.AddToRoleAsync(user, UserRoles.Customer)
///    - Kontrollera roller: User.IsInRole(UserRoles.Administrator)
///    - Skydda controller eller metod: [Authorize(Roles = UserRoles.Administrator)]
/// 
/// 4. Vid användarregistrering:
///    - Nya användare får automatiskt Customer-rollen
///    - Admin användare kan skapas med seedning
/// </summary>
public static class UserRoles
{
    public const string Administrator = "Admin";
    public const string Customer = "Customer";

    public static IEnumerable<string> AllRoles => new[] { Administrator, Customer };
}