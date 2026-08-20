using Construction.Core.DTOs;
using Construction.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Construction.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MilestonesController : ControllerBase
{
    private readonly IMilestoneService _service;

    public MilestonesController(IMilestoneService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResponseDto<MilestoneDto>>> GetMilestones([FromQuery] QueryParametersDto query, CancellationToken ct)
        => Ok(await _service.GetMilestonesAsync(query, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MilestoneDto>> GetMilestone(int id, CancellationToken ct)
    {
        var milestone = await _service.GetMilestoneByIdAsync(id, ct);
        return milestone is null ? NotFound() : Ok(milestone);
    }

    [HttpGet("upcoming")]
    public async Task<ActionResult<IReadOnlyList<UpcomingMilestoneDto>>> GetUpcomingMilestones([FromQuery] int days = 30, [FromQuery] int limit = 20, CancellationToken ct = default)
        => Ok(await _service.GetUpcomingMilestonesAsync(days, limit, ct));

    // NOTE: Read-only public surface — see BudgetsController for rationale.
}
