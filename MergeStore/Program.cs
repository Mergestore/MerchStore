using MergeStore.Configurations;
using MergeStore.Repositories;
using MergeStore.Services;
using MergeStore.Storage;
using MongoDB.Driver;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Set Swedish culture as default
var swedishCulture = new CultureInfo("sv-SE");
CultureInfo.DefaultThreadCurrentCulture = swedishCulture;
CultureInfo.DefaultThreadCurrentUICulture = swedishCulture;

// Registrera IHttpContextAccessor för att få tillgång till HttpContext i tjänster
builder.Services.AddHttpContextAccessor();

// Välj implementering av IImageService baserat på miljö
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IImageService, LocalImageService>();
}
else
{
    builder.Services.AddSingleton<IImageService, AzureBlobImageService>();
}

// Använd endast in-memory repositories för enkel testning
builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();