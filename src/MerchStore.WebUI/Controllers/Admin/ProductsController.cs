using MerchStore.Application.Common.Interfaces;
using MerchStore.Domain.Interfaces;
using MerchStore.WebUI.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MerchStore.WebUI.Controllers.Admin;

/// <summary>
/// Controller för hantering av produkter i admin-gränssnittet.
/// Alla actions är skyddade med Admin-roll.
/// </summary>
[Authorize(Roles = "Admin")]
[Route("Admin/Products")]
public class ProductsController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly IWebHostEnvironment _env;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductsController> _logger;

    /// <summary>
    /// Konstruktor med dependency injection av nödvändiga tjänster
    /// </summary>
    public ProductsController(
        IProductRepository productRepository,
        IWebHostEnvironment env,
        IUnitOfWork unitOfWork,
        ILogger<ProductsController> logger)
    {
        _productRepository = productRepository;
        _env = env;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Visar listan över alla produkter för admin
    /// GET: /Admin/Products
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("Hämtar alla produkter för admin-vyn");
        var products = await _productRepository.GetAllAsync();
        return View("~/Views/Admin/Products/Index.cshtml", products);
    }

    /// <summary>
    /// Visar formulär för att lägga till produkt
    /// GET: /Admin/Products/Create
    /// </summary>
    [HttpGet("Create")]
    public IActionResult Create()
    {
        _logger.LogInformation("Visar formulär för att skapa ny produkt");
        return View("~/Views/Admin/Products/Create.cshtml");
    }

    /// <summary>
    /// Hanterar skapande av ny produkt med eventuell bild
    /// POST: /Admin/Products/Create
    /// </summary>
    [HttpPost("Create")]
    [ValidateAntiForgeryToken] // Skyddar mot CSRF-attacker
    public async Task<IActionResult> Create(ProductFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Ogiltigt formulär vid skapande av produkt");
            return View("~/Views/Admin/Products/Create.cshtml", model);
        }

        try
        {
            _logger.LogInformation("Påbörjar skapande av produkt: {ProductName}", model.Name);

            // Hantera eventuell bilduppladdning
            string? imageUrl = null;
            if (model.ImageFile is { Length: > 0 })
            {
                // Säkerhetsvalidering av filtyp
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(model.ImageFile.ContentType))
                {
                    _logger.LogWarning("Nekad uppladdning av otillåten filtyp: {ContentType}", model.ImageFile.ContentType);
                    ModelState.AddModelError("ImageFile", "Endast bilder av typen JPEG, PNG, GIF och WEBP är tillåtna.");
                    return View("~/Views/Admin/Products/Create.cshtml", model);
                }

                // Säkerhetskontroll av filstorlek
                if (model.ImageFile.Length > 5 * 1024 * 1024) // 5 MB
                {
                    _logger.LogWarning("Nekad uppladdning av för stor fil: {Size} bytes", model.ImageFile.Length);
                    ModelState.AddModelError("ImageFile", "Bildstorleken får inte överstiga 5 MB.");
                    return View("~/Views/Admin/Products/Create.cshtml", model);
                }

                // Skapa mapp för produktbilder om den inte finns
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "products");
                if (!Directory.Exists(uploadsFolder))
                {
                    _logger.LogInformation("Skapar katalog för produktbilder: {Path}", uploadsFolder);
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generera unikt filnamn för att undvika konflikter
                var extension = Path.GetExtension(model.ImageFile.FileName);
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                _logger.LogInformation("Sparar bild till: {FilePath}", filePath);

                // Spara filen
                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                // Sätt URL för användning i webben - relativ till webbrot
                imageUrl = $"/images/products/{fileName}";
                _logger.LogInformation("Produktbild har sparats med URL: {ImageUrl}", imageUrl);
            }

            // Konvertera URL till URI för att skapa produkt-entity
            Uri? imageUri = null;
            if (!string.IsNullOrEmpty(imageUrl))
            {
                imageUri = new Uri(imageUrl, UriKind.Relative);
            }

            // Skapa produkt-entity
            var product = new Domain.Entities.Product(
                model.Name,
                model.Description,
                imageUri,
                new Domain.ValueObjects.Money(model.Price, model.Currency),
                model.StockQuantity
            );

            // Spara produkt till databasen - två steg:
            // 1. Lägg till produkt i repository (spårar ändringar)
            // 2. Spara ändringar till databasen via UnitOfWork
            await _productRepository.AddAsync(product);
            var changeResult = await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Produkt '{ProductName}' har skapats med ID: {ProductId}. Ändringar: {Changes}",
                product.Name, product.Id, changeResult);

            TempData["SuccessMessage"] = $"Produkten '{product.Name}' har lagts till.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid skapande av produkt: {ProductName}", model.Name);
            ModelState.AddModelError("", $"Ett fel uppstod: {ex.Message}");
            return View("~/Views/Admin/Products/Create.cshtml", model);
        }
    }

    /// <summary>
    /// Visar formulär för att redigera produkt
    /// GET: /Admin/Products/Edit/{id}
    /// </summary>
    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        _logger.LogInformation("Hämtar produkt för redigering: {ProductId}", id);

        // Hämta produkten från databasen
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Produkt hittades inte: {ProductId}", id);
            TempData["ErrorMessage"] = "Produkten hittades inte.";
            return NotFound();
        }

        // Skapa en view model baserad på produkten
        var viewModel = new ProductFormViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price.Amount,
            Currency = product.Price.Currency,
            StockQuantity = product.StockQuantity,
            ExistingImageUrl = product.ImageUrl?.ToString()
        };

        return View("~/Views/Admin/Products/Edit.cshtml", viewModel);
    }

    /// <summary>
    /// Hanterar uppdatering av produkt och eventuell bild
    /// POST: /Admin/Products/Edit/{id}
    /// 
    /// OBS: Denna metod använder ett "ta bort och återskapa"-mönster istället för en 
    /// vanlig uppdatering eftersom domänmodellen har privata setters, vilket 
    /// gör det svårt att uppdatera entiteter med Entity Framework.
    /// </summary>
    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken] // Skyddar mot CSRF-attacker
    public async Task<IActionResult> Edit(Guid id, ProductFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Ogiltigt formulär vid redigering av produkt {ProductId}", id);
            return View("~/Views/Admin/Products/Edit.cshtml", model);
        }

        // ENKEL APPROACH: Skapa en helt ny produkt och ersätt den befintliga
        try
        {
            _logger.LogInformation("Påbörjar redigering av produkt: {ProductId} - {ProductName}", id, model.Name);

            // Hantera bilduppladdning om det finns
            string? imageUrl = model.ExistingImageUrl;  // Använd befintlig URL som standard

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                // Säkerhetsvalidering av filtyp
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(model.ImageFile.ContentType))
                {
                    _logger.LogWarning("Nekad uppladdning av otillåten filtyp: {ContentType}", model.ImageFile.ContentType);
                    ModelState.AddModelError("ImageFile", "Endast bilder av typen JPEG, PNG, GIF och WEBP är tillåtna.");
                    return View("~/Views/Admin/Products/Edit.cshtml", model);
                }

                // Säkerhetskontroll av filstorlek
                if (model.ImageFile.Length > 5 * 1024 * 1024) // 5 MB
                {
                    _logger.LogWarning("Nekad uppladdning av för stor fil: {Size} bytes", model.ImageFile.Length);
                    ModelState.AddModelError("ImageFile", "Bildstorleken får inte överstiga 5 MB.");
                    return View("~/Views/Admin/Products/Edit.cshtml", model);
                }

                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "products");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var extension = Path.GetExtension(model.ImageFile.FileName);
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                _logger.LogInformation("Sparar ny bild för produkt {ProductId} till: {FilePath}", id, filePath);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                // Sätt ny bildadress - relativ till webbrot
                imageUrl = $"/images/products/{fileName}";
                _logger.LogInformation("Ny produktbild har sparats med URL: {ImageUrl}", imageUrl);
            }

            _logger.LogInformation("Skapar ny produkt med befintligt ID={ProductId}, Namn={ProductName}", id, model.Name);

            // Ta bort den befintliga produkten först
            var existingProduct = await _productRepository.GetByIdAsync(id);
            if (existingProduct is not null)
            {
                _logger.LogInformation("Tar bort befintlig produkt: {ProductId} - {ProductName}",
                    existingProduct.Id, existingProduct.Name);

                await _productRepository.RemoveAsync(existingProduct);
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                _logger.LogWarning("Produkten som skulle redigeras hittades inte: {ProductId}", id);
            }

            // Konvertera sträng-URL till URI om det behövs
            Uri? imageUri = null;
            if (!string.IsNullOrEmpty(imageUrl))
            {
                try
                {
                    // Hantera både relativa och absoluta URI:er - ger flexibilitet
                    // och motståndskraft mot olika format på URL
                    if (Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
                    {
                        imageUri = new Uri(imageUrl);
                        _logger.LogInformation("Använder absolut URI för bild: {ImageUrl}", imageUrl);
                    }
                    else
                    {
                        imageUri = new Uri(imageUrl, UriKind.Relative);
                        _logger.LogInformation("Använder relativ URI för bild: {ImageUrl}", imageUrl);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Kunde inte konvertera URL till URI: {ImageUrl}", imageUrl);
                    // Fortsätt utan bild om det inte går att konvertera
                }
            }

            // Skapa ny produkt med uppdaterade värden
            var product = new Domain.Entities.Product(
                model.Name,
                model.Description,
                imageUri,
                new Domain.ValueObjects.Money(model.Price, model.Currency),
                model.StockQuantity
            );

            // Sätt ID till samma som innan för att bevara identiteten
            // Reflection krävs eftersom ID har privat setter i domänmodellen
            var entityType = typeof(Domain.Common.Entity<Guid>);
            var idProperty = entityType.GetProperty("Id", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (idProperty != null)
            {
                idProperty.SetValue(product, id);
                _logger.LogInformation("ID satt till {ProductId} på ny produkt", id);
            }
            else
            {
                _logger.LogWarning("Kunde inte sätta ID på ny produkt - Id-property hittades inte");
            }

            // Lägg till den nya produkten i databasen
            await _productRepository.AddAsync(product);
            var result = await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Sparad uppdaterad produkt med ID={ProductId}, resultat: {Result}", id, result);

            // Bekräfta med meddelande till användaren
            TempData["SuccessMessage"] = $"Produkten '{model.Name}' har uppdaterats.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WARNING: Vid uppdatering av produkt {ProductId}: {ErrorMessage}", id, ex.Message);
            ModelState.AddModelError("", $"Ett fel uppstod: {ex.Message}");
            return View("~/Views/Admin/Products/Edit.cshtml", model);
        }
    }

    /// <summary>
    /// Hanterar borttagning av produkt
    /// POST: /Admin/Products/Delete/{id}
    /// </summary>
    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken] // Skyddar mot CSRF-attacker (Cross-Site Request Forgery)
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("Begär borttagning av produkt: {ProductId}", id);

        // Hämta befintlig produkt
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Försökte ta bort produkt som inte finns: {ProductId}", id);
            TempData["ErrorMessage"] = "Produkten hittades inte.";
            return NotFound();
        }

        try
        {
            _logger.LogInformation("Tar bort produkt: {ProductId} - {ProductName}", id, product.Name);

            // Spara produktnamn för meddelandet innan den tas bort
            var productName = product.Name;

            // Ta bort produkten från databasen
            await _productRepository.RemoveAsync(product);
            var result = await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Produkt borttagen: {ProductId} - {ProductName}, resultat: {Result}",
                id, productName, result);

            // Ta bort bilden från filsystemet (om det finns en)
            if (product.ImageUrl != null)
            {
                try
                {
                    var imagePath = product.ImageUrl.LocalPath.TrimStart('/');
                    var fullPath = Path.Combine(_env.WebRootPath, imagePath);

                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                        _logger.LogInformation("Produktbild borttagen: {FilePath}", fullPath);
                    }
                }
                catch (Exception ex)
                {
                    // Logga men fortsätt - det är inte kritiskt om bilden inte kan tas bort
                    _logger.LogWarning(ex, "Kunde inte ta bort bildfil för produkt {ProductId}", id);
                }
            }

            // Sätt bekräftelsemeddelande
            TempData["SuccessMessage"] = $"Produkten '{productName}' har tagits bort.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid borttagning av produkt {ProductId}: {ErrorMessage}", id, ex.Message);
            TempData["ErrorMessage"] = $"Ett fel uppstod när produkten skulle tas bort: {ex.Message}";
        }

        return RedirectToAction("Index");
    }
}