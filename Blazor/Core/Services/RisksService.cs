using Construction.Core.DTOs;

namespace Construction.Blazor.Core.Services;

public class RisksService(ApiClient api)
{
    public Task<PagedResponseDto<RiskDto>> GetRisksAsync(int page = 1, int pageSize = 50) =>
        api.GetJsonAsync<PagedResponseDto<RiskDto>>("risks", new Dictionary<string, object?> { ["page"] = page, ["pageSize"] = pageSize });

    public Task<RiskKpisDto> GetKpisAsync() =>
        api.GetJsonAsync<RiskKpisDto>("risks/kpis");

    // NOTE: This showcase exposes a read-only API surface. Update/Create/Delete are intentionally
    // not wired here to avoid anonymous write/delete vectors on the demo database. See README.

    public Task<RiskMatrixDto> GetMatrixAsync() =>
        api.GetJsonAsync<RiskMatrixDto>("risks/matrix");
}
