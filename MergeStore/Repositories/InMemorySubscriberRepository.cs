using MergeStore.Models;
using System.Collections.Concurrent;

namespace MergeStore.Repositories;

public class InMemorySubscriberRepository : ISubscriberRepository
{
    // Using ConcurrentDictionary for thread safety
    private readonly ConcurrentDictionary<string, CustomerCartInfo> _subscribers = new(StringComparer.OrdinalIgnoreCase);

    public Task<IEnumerable<CustomerCartInfo>> GetAllAsync()
    {
        return Task.FromResult(_subscribers.Values.AsEnumerable());
    }

    public Task<CustomerCartInfo?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return Task.FromResult<CustomerCartInfo?>(null);
        }

        _subscribers.TryGetValue(email, out var subscriber);
        return Task.FromResult(subscriber);
    }

    public Task<bool> AddAsync(CustomerCartInfo subscriber)
    {
        if (subscriber == null || string.IsNullOrEmpty(subscriber.Email))
        {
            return Task.FromResult(false);
        }

        // TryAdd returns true if the key was added, false if it already exists
        return Task.FromResult(_subscribers.TryAdd(subscriber.Email, subscriber));
    }

    public Task<bool> UpdateAsync(CustomerCartInfo subscriber)
    {
        if (subscriber == null || string.IsNullOrEmpty(subscriber.Email))
        {
            return Task.FromResult(false);
        }

        // We need to handle the update manually since we need to check if key exists first
        if (!_subscribers.ContainsKey(subscriber.Email))
        {
            return Task.FromResult(false);
        }

        // Using AddOrUpdate to ensure thread safety
        _subscribers.AddOrUpdate(
            subscriber.Email,
            subscriber,
            (key, oldValue) => subscriber
        );

        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return Task.FromResult(false);
        }

        // TryRemove returns true if the item was removed
        return Task.FromResult(_subscribers.TryRemove(email, out _));
    }

    public Task<bool> ExistsAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(_subscribers.ContainsKey(email));
    }
}