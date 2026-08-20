using Construction.Core.DTOs;

namespace Construction.Core.Interfaces;

// Read-only service surface for the public showcase API.
// Create/Update/Delete are intentionally not exposed. See BudgetsController for rationale.
public interface IDocumentService
{
    Task<PagedResponseDto<DocumentDto>> GetDocumentsAsync(QueryParametersDto query, CancellationToken ct = default);
    Task<DocumentDto?> GetDocumentByIdAsync(int id, CancellationToken ct = default);
}
