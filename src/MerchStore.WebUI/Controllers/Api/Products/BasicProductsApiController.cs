using Microsoft.AspNetCore.Mvc;
using MerchStore.WebUI.Models.Api.Basic;
using MerchStore.Application.Services.Interfaces;

namespace MerchStore.WebUI.Controllers.Api.Products;

/// <summary>
/// Basic API controller for read-only product operations.
/// Provides simple endpoints for listing and retrieving products.
/// </summary>
[Route("api/basic/products")]
[ApiController]
public class BasicProductsApiController : ControllerBase
{
    private readonly ICatalogService _catalogService;

    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    /// <param name="catalogService">The catalog service for accessing product data</param>
    public BasicProductsApiController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    /// <summary>
    /// Gets all products
    /// </summary>
    /// <returns>A list of all products</returns>
    /// <response code="200">Returns the list of products</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BasicProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            // Get all products from the service
            var products = await _catalogService.GetAllProductsAsync();

            // Map domain entities to DTOs
            var productDtos = products.Select(p => new BasicProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price.Amount,
                Currency = p.Price.Currency,
                ImageUrl = p.ImageUrl?.ToString(),
                StockQuantity = p.StockQuantity
            });

            // Return 200 OK with the list of products
            return Ok(productDtos);
        }
        catch
        {
            // Log the exception in a real application

            // Return 500 Internal Server Error
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving products" });
        }
    }

    /// <summary>
    /// Gets a specific product by ID
    /// </summary>
    /// <param name="id">The ID of the product to retrieve</param>
    /// <returns>The requested product</returns>
    /// <response code="200">Returns the requested product</response>
    /// <response code="404">If the product is not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BasicProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            // Get the specific product from the service
            var product = await _catalogService.GetProductByIdAsync(id);

            // Return 404 Not Found if the product doesn't exist
            if (product is null)
            {
                return NotFound(new { message = $"Product with ID {id} not found" });
            }

            // Map domain entity to DTO
            var productDto = new BasicProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price.Amount,
                Currency = product.Price.Currency,
                ImageUrl = product.ImageUrl?.ToString(),
                StockQuantity = product.StockQuantity
            };

            // Return 200 OK with the product
            return Ok(productDto);
        }
        catch
        {
            // Log the exception in a real application

            // Return 500 Internal Server Error
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving the product" });
        }
    }
}