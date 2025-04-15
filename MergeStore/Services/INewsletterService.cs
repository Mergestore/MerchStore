using MergeStore.Models;

namespace MergeStore.Services;

public interface INewsletterService
{
    Task<OperationResult> SignUpForNewsletterAsync(CustomerCartInfo subscriber);
    Task<OperationResult> OptOutFromNewsletterAsync(string email);
    Task<IEnumerable<CustomerCartInfo>> GetActiveSubscribersAsync();
}