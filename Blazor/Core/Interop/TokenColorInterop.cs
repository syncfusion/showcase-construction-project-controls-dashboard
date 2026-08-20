using Microsoft.JSInterop;

namespace Construction.Blazor.Core.Interop;

/// <summary>
/// Reads the resolved value of a CSS custom property from the document root
/// (or any element). Lets Blazor components mirror the Angular/React ports
/// in driving Syncfusion chart/grid styling from the in-house design tokens
/// instead of hard-coding hex colors. Syncfusion SVG ignores CSS variables,
/// so each color must be resolved to a literal string before being passed
/// to a chart series, axis label, tooltip, etc.
/// </summary>
public class TokenColorInterop(IJSRuntime js)
{
    /// <summary>
    /// Returns the resolved value of the given CSS variable on the document
    /// element. Returns <paramref name="fallback"/> if the property is undefined.
    /// </summary>
    public async Task<string> GetTokenColorAsync(string tokenName, string fallback)
    {
        try
        {
            var value = await js.InvokeAsync<string?>("tokenInterop.getCssVar", tokenName);
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }
        catch
        {
            return fallback;
        }
    }
}
