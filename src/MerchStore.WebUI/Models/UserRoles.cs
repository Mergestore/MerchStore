namespace MerchStore.WebUI.Models;

public static class UserRoles
{
    public const string Administrator = "Administrator";
    public const string Customer = "Customer";

    public static IEnumerable<string> AllRoles => new[] { Administrator, Customer };
}