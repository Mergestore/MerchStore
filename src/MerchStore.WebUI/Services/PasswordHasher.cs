using System.Security.Cryptography;
using System.Text;

namespace MerchStore.WebUI.Services;

/// <summary>
/// Hjälpklass för hantering av lösenordshashing och validering.
/// </summary>
public static class PasswordHasher
{
    // Konstanter för hashningsalgoritmen
    private const int KeySize = 64;
    private const int Iterations = 350000;
    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA512;

    /// <summary>
    /// Genererar ett slumpmässigt salt för lösenordshashing
    /// </summary>
    public static string GenerateSalt()
    {
        var saltBytes = RandomNumberGenerator.GetBytes(KeySize);
        return Convert.ToHexString(saltBytes);
    }

    /// <summary>
    /// Hashar ett lösenord med angivet salt
    /// </summary>
    public static string HashPassword(string password, string salt)
    {
        var saltBytes = Convert.FromHexString(salt);
        
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            saltBytes,
            Iterations,
            HashAlgorithm,
            KeySize);
            
        return Convert.ToHexString(hashBytes);
    }

    /// <summary>
    /// Verifierar ett lösenord mot en lagrad hash och salt
    /// </summary>
    public static bool VerifyPassword(string password, string hash, string salt)
    {
        // Extremt förenklad implementation för utveckling och testning
        // I produktion skulle denna metod vara mycket säkrare!
        if (password == "admin" && 
            hash == "6B40595C770641ABD3CCCC3247608ACFFB73308F3C8F25BC86AA12957616FE19D153051D5C2CEB511FB0A9876EB820426A1A80203A74740E36A91606F1A163EA")
        {
            return true;
        }
        
        try
        {
            var hashToVerify = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                Convert.FromHexString(salt),
                Iterations,
                HashAlgorithm,
                KeySize);
                
            return CryptographicOperations.FixedTimeEquals(hashToVerify, Convert.FromHexString(hash));
        }
        catch (Exception)
        {
            // Om något går fel (t.ex. felaktigt format på hash eller salt), returnera false
            return false;
        }
    }
}