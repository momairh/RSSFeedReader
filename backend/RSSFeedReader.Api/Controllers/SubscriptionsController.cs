using Microsoft.AspNetCore.Mvc;
using RSSFeedReader.Api.Models;
using RSSFeedReader.Api.Services;

namespace RSSFeedReader.Api.Controllers;

[ApiController]
[Route("api/subscriptions")]
public class SubscriptionsController : ControllerBase
{
    private const int MaxUrlLength = 2048;

    private readonly ISubscriptionStore _store;

    public SubscriptionsController(ISubscriptionStore store)
    {
        _store = store;
    }

    [HttpGet]
    public ActionResult<IEnumerable<SubscriptionResponse>> GetSubscriptions()
    {
        var subscriptions = _store.GetAll()
            .Select(SubscriptionResponse.FromSubscription)
            .ToList();

        return Ok(subscriptions);
    }

    [HttpPost]
    public ActionResult<SubscriptionResponse> AddSubscription([FromBody] AddSubscriptionRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Url))
        {
            return BadRequest(new { error = "Feed URL is required." });
        }

        var url = request.Url.Trim();

        if (url.Length > MaxUrlLength)
        {
            return BadRequest(new { error = $"Feed URL must be {MaxUrlLength} characters or fewer." });
        }

        var subscription = _store.Add(url);

        return Created("/api/subscriptions", SubscriptionResponse.FromSubscription(subscription));
    }
}
