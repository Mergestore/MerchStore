using System.Security.Cryptography;
using System.Text;

namespace MerchStore.WebUI.Services;

/// <summary>
/// Hjälpklass för hantering av lösenordshashing och validering med BCrypt.
/// </summary>
public static class PasswordHasher
{
    // Arbetsfaktor för BCrypt (högre värde = säkrare men långsammare)
    private const int WorkFactor = 12;

    /// <summary>
    /// Hashar ett lösenord med BCrypt och genererar automatiskt ett salt
    /// </summary>
    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    /// <summary>
    /// Genererar ett salt för bakåtkompatibilitet
    /// </summary>
    public static string GenerateSalt()
    {
        // Salt genereras automatiskt i BCrypt.HashPassword
        return BCrypt.Net.BCrypt.GenerateSalt(WorkFactor);
    }

    /// <summary>
    /// Verifierar ett lösenord mot en lagrad BCrypt-hash
    /// </summary>
    public static bool VerifyPassword(string password, string hashedPassword)
    {
        Console.WriteLine($"DEBUG: Försöker verifiera lösenord mot hash: {hashedPassword}");

        try
        {
            var result = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            Console.WriteLine($"DEBUG: BCrypt-verifiering resultat: {result}");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DEBUG: BCrypt-fel: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Kontrollerar om en hash behöver uppgraderas (t.ex. om arbetsfaktorn ändrats)
    /// </summary>
    public static bool NeedsUpgrade(string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.PasswordNeedsRehash(hash, WorkFactor);
        }
        catch
        {
            // Om hash är i gammalt format behöver den definitivt uppgraderas
            return true;
        }
    }
}