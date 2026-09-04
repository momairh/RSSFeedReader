using System.Net.Http.Json;
using System.Text.Json;
using RSSFeedReader.UI.Models;

namespace RSSFeedReader.UI.Services;

public class SubscriptionApiClient : ISubscriptionApiClient
{
    private const string SubscriptionsPath = "subscriptions";
    private const string ConnectionErrorMessage =
        "Could not reach the RSS Feed Reader service. Make sure the backend is running.";

    private readonly HttpClient _httpClient;

    public SubscriptionApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<SubscriptionResponse>> GetSubscriptionsAsync()
    {
        try
        {
            var subscriptions = await _httpClient.GetFromJsonAsync<List<SubscriptionResponse>>(SubscriptionsPath);
            return subscriptions ?? new List<SubscriptionResponse>();
        }
        catch (HttpRequestException)
        {
            throw new SubscriptionApiException(ConnectionErrorMessage);
        }
        catch (JsonException)
        {
            throw new SubscriptionApiException("The service returned an unexpected response.");
        }
    }

    public async Task<SubscriptionResponse> AddSubscriptionAsync(string url)
    {
        HttpResponseMessage response;

        try
        {
            response = await _httpClient.PostAsJsonAsync(SubscriptionsPath, new { url });
        }
        catch (HttpRequestException)
        {
            throw new SubscriptionApiException(ConnectionErrorMessage);
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new SubscriptionApiException(await ReadErrorMessageAsync(response));
        }

        var created = await response.Content.ReadFromJsonAsync<SubscriptionResponse>();

        return created ?? throw new SubscriptionApiException("The service returned an unexpected response.");
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ValidationError>();
            if (!string.IsNullOrWhiteSpace(error?.Error))
            {
                return error.Error;
            }
        }
        catch (JsonException)
        {
            // Fall through to the generic message below.
        }

        return "The subscription could not be added. Please try again.";
    }

    private sealed class ValidationError
    {
        public string? Error { get; set; }
    }
}
