using Construction.Core.Entities;

namespace Construction.Core.Interfaces;

public interface IRfiRepository
{
    IQueryable<RFI> Query();
    Task<RFI?> GetByIdAsync(int id, CancellationToken ct = default);
}
