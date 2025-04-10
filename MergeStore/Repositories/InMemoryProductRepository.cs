using MergeStore.Models;
using System.Collections.Concurrent;

namespace MergeStore.Repositories
{
    public class InMemoryProductRepository : IProductRepository
    {
        // Thread-safe dictionary to store products
        // using ConcurrentDictionary to allow concurrent access
        // and modifications without locking
        private readonly ConcurrentDictionary<string, Product> _products = new();
        // Counter to generate unique IDs for products
        private int _counter = 1;

        public InMemoryProductRepository()
        {
            // Adding sample products to the in-memory repository
            AddSampleProducts();
        }

        // Method to add sample products to the in-memory repository
        private void AddSampleProducts()
        {
            var products = new List<Product>
            {
                // CoPilot generated sample products
                new Product
                {
                    Id = GetNextId(),
                    Name = "T-shirt",
                    Description = "En bekväm t-shirt i 100% bomull",
                    Price = 199.99m,
                    StockQuantity = 100,
                    IsActive = true
                },
                new Product
                {
                    Id = GetNextId(),
                    Name = "Hoodie",
                    Description = "Varm och skön huvtröja",
                    Price = 499.99m,
                    StockQuantity = 50,
                    IsActive = true
                },
                new Product
                {
                    Id = GetNextId(),
                    Name = "Keps",
                    Description = "Stilren keps med broderad logotyp",
                    Price = 249.99m,
                    StockQuantity = 75,
                    IsActive = true
                }
            };

            foreach (var product in products)
            {
                _products.TryAdd(product.Id!, product);
            }
        }

        // Method to generate a unique ID for a new product
        private string GetNextId()
        {
            return (_counter++).ToString();
        }

        // Methods to implement the IProductRepository interface
        public Task<IEnumerable<Product>> GetAllAsync()
        {
            return Task.FromResult(_products.Values.Where(p => p.IsActive).AsEnumerable());
        }

        // Method to get a product by its ID
        public Task<Product?> GetByIdAsync(string id)
        {
            _products.TryGetValue(id, out var product);
            return Task.FromResult(product);
        }

        // Method to add a new product
        public Task<bool> AddAsync(Product product)
        {
            if (product.Id == null)
            {
                product.Id = GetNextId();
            }

            return Task.FromResult(_products.TryAdd(product.Id, product));
        }

        // Method to update an existing product
        public Task<bool> UpdateAsync(Product product)
        {
            if (product.Id == null || !_products.ContainsKey(product.Id))
            {
                return Task.FromResult(false);
            }

            _products[product.Id] = product;
            return Task.FromResult(true);
        }

        // Method to delete a product by its ID
        // Returns true if the product was successfully deleted
        public Task<bool> DeleteAsync(string id)
        {
            return Task.FromResult(_products.TryRemove(id, out _));
        }
    }
}