/*  KOMMENTERAR BORT SÅLÄNGE, FÖR DEMOT. 
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MerchStore.Infrastructure.Persistence;
using MerchStore.Domain.Interfaces;
using MerchStore.Infrastructure.Persistence.Repositories;

namespace MerchStore.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        
        services.AddDbContext<AppDbContext>(options =>
        {
            // HÄR BYTER DU TILL INMEMORY (TILLFÄLLIGT FÖR DEMO)
            //options.UseInMemoryDatabase("MerchStore");
        }); 
        
        
         services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        

        // Lägg till andra tjänster här (repositories, blob storage, etc)

        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}
*/