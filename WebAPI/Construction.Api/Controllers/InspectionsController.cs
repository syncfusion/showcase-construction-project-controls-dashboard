using Construction.Core.DTOs;
using Construction.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Construction.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InspectionsController : ControllerBase
{
    private readonly IInspectionService _service;

    public InspectionsController(IInspectionService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResponseDto<InspectionDto>>> GetInspections([FromQuery] QueryParametersDto query, CancellationToken ct)
        => Ok(await _service.GetInspectionsAsync(query, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InspectionDto>> GetInspection(int id, CancellationToken ct)
    {
        var inspection = await _service.GetInspectionByIdAsync(id, ct);
        return inspection is null ? NotFound() : Ok(inspection);
    }

    // NOTE: Read-only public surface — see BudgetsController for rationale.
}
