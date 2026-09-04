using RSSFeedReader.Api.Models;

namespace RSSFeedReader.Api.Services;

public class InMemorySubscriptionStore : ISubscriptionStore
{
    private readonly List<Subscription> _subscriptions = new();
    private readonly object _syncRoot = new();

    public IReadOnlyList<Subscription> GetAll()
    {
        lock (_syncRoot)
        {
            return _subscriptions.ToList();
        }
    }

    public Subscription Add(string url)
    {
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            Url = url.Trim(),
            AddedAt = DateTimeOffset.UtcNow
        };

        lock (_syncRoot)
        {
            _subscriptions.Add(subscription);
        }

        return subscription;
    }
}
