namespace Construction.Maui.Models;

/// <summary>Lightweight chart segment model for bound donut/pie series.</summary>
public sealed class ChartSegmentVm
{
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
}
