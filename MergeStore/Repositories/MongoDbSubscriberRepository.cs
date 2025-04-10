using MergeStore.Models;
using MongoDB.Driver;

namespace MergeStore.Repositories
{
    public class MongoDbProductRepository : IProductRepository
    {
        private readonly IMongoCollection<Product> _products;

        public MongoDbProductRepository(IMongoCollection<Product> products)
        {
            _products = products;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _products.Find(p => p.IsActive).ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(string id)
        {
            return await _products.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<bool> AddAsync(Product product)
        {
            try
            {
                await _products.InsertOneAsync(product);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Product product)
        {
            try
            {
                var result = await _products.ReplaceOneAsync(
                    p => p.Id == product.Id,
                    product,
                    new ReplaceOptions { IsUpsert = false });

                return result.ModifiedCount > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var result = await _products.DeleteOneAsync(p => p.Id == id);
                return result.DeletedCount > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}