using Construction.Core.DTOs;

namespace Construction.Blazor.Core.Services;

public class ChangeOrdersService(ApiClient api)
{
    public Task<PagedResponseDto<ChangeOrderSummaryDto>> GetChangeOrdersAsync(int page = 1, int pageSize = 50) =>
        api.GetJsonAsync<PagedResponseDto<ChangeOrderSummaryDto>>("changeorders", new Dictionary<string, object?> { ["page"] = page, ["pageSize"] = pageSize });
}
