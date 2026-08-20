using Construction.Core.DTOs;
using Construction.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Construction.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _service;

    public DocumentsController(IDocumentService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResponseDto<DocumentDto>>> GetDocuments([FromQuery] QueryParametersDto query, CancellationToken ct)
        => Ok(await _service.GetDocumentsAsync(query, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DocumentDto>> GetDocument(int id, CancellationToken ct)
    {
        var doc = await _service.GetDocumentByIdAsync(id, ct);
        return doc is null ? NotFound() : Ok(doc);
    }

    // NOTE: Read-only public surface — see BudgetsController for rationale.
}
