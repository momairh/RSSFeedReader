using RSSFeedReader.Api.Models;

namespace RSSFeedReader.Api.Services;

public interface ISubscriptionStore
{
    IReadOnlyList<Subscription> GetAll();

    Subscription Add(string url);
}
