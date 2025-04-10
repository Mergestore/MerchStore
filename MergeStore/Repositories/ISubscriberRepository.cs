using MergeStore.Models;

namespace MergeStore.Repositories;

public interface ISubscriberRepository
{
    Task<IEnumerable<CustomerCartInfo>> GetAllAsync();
    Task<CustomerCartInfo?> GetByEmailAsync(string email);
    Task<bool> AddAsync(CustomerCartInfo subscriber);
    Task<bool> UpdateAsync(CustomerCartInfo subscriber);
    Task<bool> DeleteAsync(string email);
    Task<bool> ExistsAsync(string email);
}