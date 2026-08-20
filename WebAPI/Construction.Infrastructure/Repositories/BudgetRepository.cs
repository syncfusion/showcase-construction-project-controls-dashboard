using Construction.Core.Entities;
using Construction.Core.Interfaces;
using Construction.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Construction.Infrastructure.Repositories;

public class BudgetRepository : IBudgetRepository
{
    private readonly ConstructionDbContext _db;
    public BudgetRepository(ConstructionDbContext db) => _db = db;

    public IQueryable<Budget> Query() => _db.Budgets.AsNoTracking().Include(b => b.CostItems);

    public Task<Budget?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Budgets.AsNoTracking().Include(b => b.CostItems).FirstOrDefaultAsync(b => b.Id == id, ct);
}
