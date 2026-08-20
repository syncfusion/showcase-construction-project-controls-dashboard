using Construction.Core.Entities;
using Construction.Core.Interfaces;
using Construction.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Construction.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly ConstructionDbContext _db;
    public DocumentRepository(ConstructionDbContext db) => _db = db;

    public IQueryable<Document> Query() => _db.Documents.AsNoTracking();

    public Task<Document?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Documents.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, ct);
}
