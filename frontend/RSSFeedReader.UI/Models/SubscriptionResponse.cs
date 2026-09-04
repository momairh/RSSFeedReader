namespace RSSFeedReader.UI.Models;

public class SubscriptionResponse
{
    public Guid Id { get; set; }

    public string Url { get; set; } = string.Empty;

    public DateTimeOffset AddedAt { get; set; }
}
