using System.Text;

namespace Construction.Blazor.Core.Utils;

public record CsvColumn<T>(string Header, Func<T, object?> Value);

/// <summary>Builds CSV text client-side-equivalent to the React/Angular downloadCsv() helper —
/// the actual browser download is triggered separately via DownloadInterop, since Blazor Server
/// has no direct file-system/browser access from C#.</summary>
public static class CsvBuilder
{
    public static string Build<T>(IReadOnlyList<CsvColumn<T>> columns, IEnumerable<T> rows)
    {
        var sb = new StringBuilder();
        sb.Append(string.Join(',', columns.Select(c => Escape(c.Header))));
        foreach (var row in rows)
        {
            sb.Append("\r\n");
            sb.Append(string.Join(',', columns.Select(c => Escape(c.Value(row)))));
        }
        return sb.ToString();
    }

    private static string Escape(object? value)
    {
        if (value is null) return string.Empty;
        var text = value switch
        {
            IFormattable formattable => formattable.ToString(null, System.Globalization.CultureInfo.InvariantCulture),
            _ => value.ToString(),
        } ?? string.Empty;
        return text.IndexOfAny([',', '"', '\n']) >= 0
            ? $"\"{text.Replace("\"", "\"\"")}\""
            : text;
    }
}
