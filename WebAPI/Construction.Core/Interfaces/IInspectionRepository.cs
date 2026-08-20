using Construction.Core.Entities;

namespace Construction.Core.Interfaces;

public interface IInspectionRepository
{
    IQueryable<Inspection> Query();
    Task<Inspection?> GetByIdAsync(int id, CancellationToken ct = default);
}
