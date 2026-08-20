using Construction.Core.Entities;

namespace Construction.Core.Interfaces;

public interface IChangeOrderRepository
{
    IQueryable<ChangeOrder> Query();
    Task<ChangeOrder?> GetByIdAsync(int id, CancellationToken ct = default);
}
