using Construction.Core.Entities;
using Construction.Core.Interfaces;
using Construction.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Construction.Infrastructure.Repositories;

public class ChangeOrderRepository : IChangeOrderRepository
{
    private readonly ConstructionDbContext _db;
    public ChangeOrderRepository(ConstructionDbContext db) => _db = db;

    public IQueryable<ChangeOrder> Query() => _db.ChangeOrders.AsNoTracking();

    public Task<ChangeOrder?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.ChangeOrders.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
}
