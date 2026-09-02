using System.Text.Json;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Construction.Maui.Services;

/// <summary>
/// Thin JSON-over-HTTP client used by all MAUI services. Mirrors the Blazor
/// ApiClient (HttpClient with named client "ConstructionApi", System.Text.Json
/// with camelCase + enum-as-string). Backend remains the single source of
/// truth — no static/fake data is returned from here.
/// </summary>
public interface IApiClient
{
    Task<T> GetJsonAsync<T>(string relativeUrl, CancellationToken ct = default);
    Task<T> PostJsonAsync<T>(string relativeUrl, object? body, CancellationToken ct = default);
    Task<T> PutJsonAsync<T>(string relativeUrl, object? body, CancellationToken ct = default);
    Task DeleteJsonAsync(string relativeUrl, CancellationToken ct = default);
}

public sealed class ApiClient : IApiClient
{
    private readonly IHttpClientFactory _factory;
    private readonly JsonSerializerOptions _json;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(IHttpClientFactory factory, JsonSerializerOptions json, ILogger<ApiClient> logger)
    {
        _factory = factory;
        _json = json;
        _logger = logger;
    }

    public async Task<T> GetJsonAsync<T>(string relativeUrl, CancellationToken ct = default)
    {
        try
        {
            using var client = _factory.CreateClient(MauiProgram.ApiHttpClientName);
            var fullUrl = client.BaseAddress?.ToString().TrimEnd('/') + "/" + relativeUrl.TrimStart('/');
            _logger.LogInformation("GET {Url}", fullUrl);
            
            using var resp = await client.GetAsync(relativeUrl, ct).ConfigureAwait(false);
            _logger.LogInformation("GET {Url} → {Status}", fullUrl, resp.StatusCode);
            
            await EnsureSuccess(resp).ConfigureAwait(false);
            return (await Deserialize<T>(resp.Content).ConfigureAwait(false))!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET {Url} failed", relativeUrl);
            throw;
        }
    }

    public async Task<T> PostJsonAsync<T>(string relativeUrl, object? body, CancellationToken ct = default)
    {
        using var client = _factory.CreateClient(MauiProgram.ApiHttpClientName);
        using var content = MakeContent(body);
        using var resp = await client.PostAsync(relativeUrl, content, ct).ConfigureAwait(false);
        await EnsureSuccess(resp).ConfigureAwait(false);
        return (await Deserialize<T>(resp.Content).ConfigureAwait(false))!;
    }

    public async Task<T> PutJsonAsync<T>(string relativeUrl, object? body, CancellationToken ct = default)
    {
        using var client = _factory.CreateClient(MauiProgram.ApiHttpClientName);
        using var content = MakeContent(body);
        using var resp = await client.PutAsync(relativeUrl, content, ct).ConfigureAwait(false);
        await EnsureSuccess(resp).ConfigureAwait(false);
        return (await Deserialize<T>(resp.Content).ConfigureAwait(false))!;
    }

    public async Task DeleteJsonAsync(string relativeUrl, CancellationToken ct = default)
    {
        using var client = _factory.CreateClient(MauiProgram.ApiHttpClientName);
        using var resp = await client.DeleteAsync(relativeUrl, ct).ConfigureAwait(false);
        await EnsureSuccess(resp).ConfigureAwait(false);
    }

    private static HttpContent MakeContent(object? body) =>
        body is null
            ? new StringContent(string.Empty, Encoding.UTF8, "application/json")
            : new StringContent(JsonSerializer.Serialize(body, MauiProgram.JsonOptions), Encoding.UTF8, "application/json");

    private static async Task EnsureSuccess(HttpResponseMessage resp)
    {
        if (resp.IsSuccessStatusCode) return;
        var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
        var message = string.IsNullOrWhiteSpace(body) ? $"HTTP {(int)resp.StatusCode}" : body;
        throw new HttpRequestException(message, null, resp.StatusCode);
    }

    private async Task<T?> Deserialize<T>(HttpContent content)
    {
        await using var stream = await content.ReadAsStreamAsync().ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync<T>(stream, _json).ConfigureAwait(false);
    }
}
