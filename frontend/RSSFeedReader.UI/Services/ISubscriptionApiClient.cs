using RSSFeedReader.UI.Models;

namespace RSSFeedReader.UI.Services;

public interface ISubscriptionApiClient
{
    Task<IReadOnlyList<SubscriptionResponse>> GetSubscriptionsAsync();

    Task<SubscriptionResponse> AddSubscriptionAsync(string url);
}
