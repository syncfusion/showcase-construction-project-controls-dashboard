using Construction.Core.Entities;

namespace Construction.Core.Interfaces;

public interface IDocumentRepository
{
    IQueryable<Document> Query();
    Task<Document?> GetByIdAsync(int id, CancellationToken ct = default);
}
