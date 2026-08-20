using Construction.Core.Entities;

namespace Construction.Core.Interfaces;

public interface ISubmittalRepository
{
    IQueryable<Submittal> Query();
    Task<Submittal?> GetByIdAsync(int id, CancellationToken ct = default);
}
