using Construction.Core.Entities;

namespace Construction.Core.Interfaces;

public interface IBudgetRepository
{
    IQueryable<Budget> Query();
    Task<Budget?> GetByIdAsync(int id, CancellationToken ct = default);
}
