using System.Globalization;

namespace Construction.Blazor.Core.Utils;

public static class Formatters
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    // InvariantCulture's "C0" format uses the generic currency placeholder symbol (¤), not
    // "$" — a real US-dollar culture is needed for the "C0" specifier to render "$".
    private static readonly CultureInfo CurrencyCulture = CultureInfo.GetCultureInfo("en-US");

    public static string FormatDate(DateTime? value)
    {
        if (value is null) return "—";
        return value.Value.ToString("MMM d, yyyy", Culture);
    }

    public static string FormatCurrency(decimal? value)
    {
        if (value is null) return "—";
        return value.Value.ToString("C0", CurrencyCulture);
    }

    /// <summary>1.23B -> $1.2B, 42.5M -> $42.5M, 949.5K -> $949.5K, under $1K falls back to full format.</summary>
    public static string FormatCompactCurrency(decimal? value)
    {
        if (value is null) return "—";
        var abs = Math.Abs(value.Value);
        var sign = value.Value < 0 ? "-" : "";
        if (abs >= 1_000_000_000m) return $"{sign}${(abs / 1_000_000_000m):0.0}B";
        if (abs >= 1_000_000m) return $"{sign}${(abs / 1_000_000m):0.0}M";
        if (abs >= 1_000m) return $"{sign}${(abs / 1_000m):0.0}K";
        return FormatCurrency(value);
    }
}
