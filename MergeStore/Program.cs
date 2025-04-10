using MergeStore.Configurations;
using MergeStore.Repositories;
using MergeStore.Services;
using MergeStore.Storage;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Konfigurera MongoDB Options
builder.Services.Configure<MongoDbOptions>(
    builder.Configuration.GetSection(MongoDbOptions.SectionName));

// Konfigurera Azure Blob Options
builder.Services.Configure<AzureBlobOptions>(
    builder.Configuration.GetSection(AzureBlobOptions.SectionName));

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

// Konfigurera MongoDB
var mongoDbOptions = builder.Configuration
    .GetSection(MongoDbOptions.SectionName)
    .Get<MongoDbOptions>();

if (mongoDbOptions != null)
{
    var mongoClient = new MongoClient(mongoDbOptions.ConnectionString);
    var database = mongoClient.GetDatabase(mongoDbOptions.DatabaseName);
var subscribersCollection = database.GetCollection<MergeStore.Models.CustomerCartInfo>(
    mongoDbOptions.SubscribersCollectionName);
    
    builder.Services.AddSingleton(subscribersCollection);
    builder.Services.AddSingleton<ISubscriberRepository, MongoDbSubscriberRepository>();
}
else
{
    // Fallback to in-memory repository if MongoDB is not configured
    builder.Services.AddSingleton<ISubscriberRepository, InMemorySubscriberRepository>();
}

// Registrera nyhetsbrevstjänst
builder.Services.AddScoped<INewsletterService, NewsletterService>();

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