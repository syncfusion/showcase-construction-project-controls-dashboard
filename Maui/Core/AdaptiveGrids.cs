using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;

namespace Construction.Maui.Core;

/// <summary>
/// Adaptive-layout helpers for the MAUI port (Phase 3, item 10).
/// Mirrors the React app's responsive grid column collapse: a multi-column
/// KPI / chart grid reflows to fewer columns below fixed width breakpoints,
/// down to a single column on phone-width screens.
///
/// The breakpoints match React's media-query values:
/// <list type="bullet">
/// <item><c>&lt; 768</c>     → 1 column</item>
/// <item><c>768..1023</c>   → 2 columns</item>
/// <item><c>1024..1439</c>  → up to the grid's <paramref name="maxColumns"/></item>
/// <item><c>&gt;= 1440</c>  → up to the grid's <paramref name="maxColumns"/></item>
/// </list>
/// Tablets/phones respect the same width buckets; <see cref="DeviceInfo.Idiom"/>
/// is NOT consulted here — the shell already forces phone→narrow widths, so a
/// narrower measured width is the correct signal for the content grids too.
///
/// <para>Re-flow also rebases each child's <c>Grid.Column</c>/<c>Grid.Row</c> in
/// row-major order so children that were pinned at <c>Grid.Column="3"</c> for the
/// desktop 4-column layout wrap correctly when the column count drops. This
/// mirrors an auto-fit grid without needing a separate grid framework.</para>
/// </summary>
public static class AdaptiveGrids
{
    private const double Narrow = 768;
    private const double Medium  = 1024;

    /// <summary>Recomputes <paramref name="grid"/>'s <see cref="Grid.ColumnDefinitions"/>
    /// to a column count appropriate for <paramref name="availableWidth"/>, capped at
    /// <paramref name="maxColumns"/>, and re-flows the grid's direct children into
    /// row-major positions for the new column count. Safe to call repeatedly: it's a
    /// no-op when the column count is unchanged. Call from a
    /// <see cref="VisualElement.SizeChanged"/> handler (or <see cref="Page.SizeChanged"/>)
    /// — pass the host's current width.</summary>
    public static void Reflow(Grid grid, double availableWidth, int maxColumns)
    {
        if (grid is null || maxColumns <= 0) return;

        var columns = availableWidth switch
        {
            < Narrow => 1,
            < Medium => Math.Min(2, maxColumns),
            _        => maxColumns,
        };

        if (grid.ColumnDefinitions.Count == columns) return;

        // Build "*,*,…,*" with the chosen count.
        var defs = new ColumnDefinitionCollection();
        for (var i = 0; i < columns; i++) defs.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions = defs;

        // Re-flow direct children row-major. Reset ColumnSpan (desktop layout may
        // have set spans assuming the wider grid) so each child occupies exactly
        // one cell in the new column count.
        var index = 0;
        foreach (var child in grid.Children)
        {
            if (child is not View v) continue;
            Grid.SetColumn(v, index % columns);
            Grid.SetRow(v, index / columns);
            Grid.SetColumnSpan(v, 1);
            index++;
        }
    }
}
