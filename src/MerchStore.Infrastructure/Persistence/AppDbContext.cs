using Microsoft.EntityFrameworkCore;
using MerchStore.Domain.Entities;
using MerchStore.Infrastructure.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace MerchStore.Infrastructure.Persistence;

/// <summary>
/// Databaskontexten som ger tillgång till databasen genom Entity Framework Core.
/// Detta är den centrala klassen i EF Core och fungerar som den primära interaktionspunkten med databasen.
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    /// <summary>
    /// DbSet representerar en samling entiteter av en specifik typ i databasen.
    /// Varje DbSet motsvarar vanligtvis en databastabell.
    /// </summary>
    public DbSet<Product> Products { get; set; }

    /// <summary>
    /// Konstruktor som accepterar DbContextOptions, vilket möjliggör konfiguration att skickas in.
    /// Detta gör att olika databasleverantörer (SQL Server, In-Memory, etc.) kan användas med samma kontext.
    /// </summary>
    /// <param name="options">Inställningarna som ska användas av DbContext</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Denna metod anropas när modellen för en härledd kontext skapas.
    /// Den möjliggör konfiguration av entiteter, relationer och andra modellbyggande aktiviteter.
    /// </summary>
    /// <param name="modelBuilder">Tillhandahåller ett enkelt API för att konfigurera modellen</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations from the current assembly
        // This scans for all IEntityTypeConfiguration implementations and applies them
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}