namespace RSSFeedReader.Api.Models;

public class SubscriptionResponse
{
    public Guid Id { get; init; }

    public string Url { get; init; } = string.Empty;

    public DateTimeOffset AddedAt { get; init; }

    public static SubscriptionResponse FromSubscription(Subscription subscription) => new()
    {
        Id = subscription.Id,
        Url = subscription.Url,
        AddedAt = subscription.AddedAt
    };
}
