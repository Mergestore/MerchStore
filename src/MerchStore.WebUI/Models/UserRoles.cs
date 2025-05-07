namespace MerchStore.WebUI.Models;

public static class UserRoles
{
    public const string Administrator = "Admin";
    public const string Customer = "Customer";

    public static IEnumerable<string> AllRoles => new[] { Administrator, Customer };
}