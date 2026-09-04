namespace RSSFeedReader.Api.Models;

public class Subscription
{
    public Guid Id { get; init; }

    public string Url { get; init; } = string.Empty;

    public DateTimeOffset AddedAt { get; init; }
}
