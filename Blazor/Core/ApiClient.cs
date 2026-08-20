using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace Construction.Blazor.Core;

/// <summary>
/// Thin JSON HTTP wrapper around the named "ConstructionApi" HttpClient — mirrors the
/// getJson/postJson/putJson/deleteJson helpers in the React/Angular api clients. Runs
/// server-side (this is a Blazor Server app), so CORS on Construction.Api never applies here.
/// </summary>
public class ApiClient(IHttpClientFactory httpClientFactory)
{
    // Construction.Api registers a JsonStringEnumConverter globally (Program.cs), so every
    // enum (HealthStatus, ProjectStatus, RiskSeverity, ...) is serialized as a string, not a
    // number — this must match on the client side or deserialization throws.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient _http = httpClientFactory.CreateClient("ConstructionApi");

    private static string BuildQuery(IReadOnlyDictionary<string, object?>? parameters)
    {
        if (parameters is null || parameters.Count == 0) return string.Empty;
        var query = HttpUtility.ParseQueryString(string.Empty);
        foreach (var (key, value) in parameters)
        {
            if (value is null) continue;
            var text = value.ToString();
            if (string.IsNullOrEmpty(text)) continue;
            query[key] = text;
        }
        var queryString = query.ToString();
        return string.IsNullOrEmpty(queryString) ? string.Empty : $"?{queryString}";
    }

    public async Task<T> GetJsonAsync<T>(string path, IReadOnlyDictionary<string, object?>? parameters = null)
    {
        var url = path + BuildQuery(parameters);
        var result = await _http.GetFromJsonAsync<T>(url, JsonOptions);
        return result ?? throw new InvalidOperationException($"Empty response from {url}");
    }

    // NOTE: This showcase exposes a read-only API surface. PostJsonAsync / PutJsonAsync /
    // DeleteAsync are intentionally not provided here to avoid anonymous write/delete vectors
    // on the demo database. See README.
}
