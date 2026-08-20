namespace Construction.Blazor.Core.Services;

/// <summary>Flattened view model for the Probability x Severity risk matrix — the server
/// returns a nested rows/cells shape (see RiskMatrixDto) with probability/severity as plain
/// strings; this is easier to look up by (probability, severity) pair when rendering the grid.</summary>
public record RiskMatrixCellViewModel(string Probability, string Severity, int Count, IReadOnlyList<string> RiskIds);

public class RiskMatrixService(RisksService risks)
{
    public async Task<List<RiskMatrixCellViewModel>> GetMatrixAsync()
    {
        var matrix = await risks.GetMatrixAsync();
        return matrix.Rows
            .SelectMany(row => row.Cells.Select(cell =>
                new RiskMatrixCellViewModel(row.Probability, cell.Severity, cell.RiskNumbers.Count, cell.RiskNumbers)))
            .ToList();
    }
}
