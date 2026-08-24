using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Charts;

namespace Construction.Blazor.Components.Pages;

public partial class Reports : ComponentBase
{
    [Inject] private ReportsService ReportsApi { get; set; } = default!;
    [Inject] private ChartPaletteService Palette { get; set; } = default!;

    private List<EvPoint> _evData = [];
    private List<CvPoint> _cvData = [];
    private bool _loading = true;
    private string? _error;
    private bool _paletteReady;

    private SfChart? _evChart;
    private SfChart? _cvChart;

    public class EvPoint
    {
        public string Month { get; set; } = "";
        public decimal Bcws { get; set; }
        public decimal Bcwp { get; set; }
        public decimal Acwp { get; set; }
    }

    public class CvPoint
    {
        public string CostCode { get; set; } = "";
        public decimal Variance { get; set; }
    }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var evTask = ReportsApi.GetEarnedValueTrendAsync(12);
            var cvTask = ReportsApi.GetCostVarianceByCostCodeAsync();
            await Task.WhenAll(evTask, cvTask);
            _evData = evTask.Result.Select(p => new EvPoint { Month = p.Month, Bcws = p.Bcws, Bcwp = p.Bcwp, Acwp = p.Acwp }).ToList();
            _cvData = cvTask.Result.Select(c => new CvPoint { CostCode = c.CostCode, Variance = c.VariancePct }).ToList();
        }
        catch (Exception ex)
        {
            _error = ex.Message;
        }
        finally
        {
            _loading = false;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_paletteReady)
        {
            await Palette.ResolveAsync();
            _paletteReady = true;
            StateHasChanged();
        }
    }
}
