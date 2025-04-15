using Microsoft.AspNetCore.Mvc;
using MergeStore.Models;
using MergeStore.Repositories;

namespace MergeStore.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;

        // Dependency injection of IProductRepository
        public ProductController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // GET: Product
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }
    }
}