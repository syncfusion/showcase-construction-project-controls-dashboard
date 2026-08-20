using Construction.Core.DTOs;
using Construction.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Construction.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _service;

    public ProjectsController(IProjectService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResponseDto<ProjectDto>>> GetProjects([FromQuery] QueryParametersDto query, CancellationToken ct)
        => Ok(await _service.GetProjectsAsync(query, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectDto>> GetProject(int id, CancellationToken ct)
    {
        var project = await _service.GetProjectByIdAsync(id, ct);
        return project is null ? NotFound() : Ok(project);
    }

    [HttpGet("{id:int}/tasks")]
    public async Task<ActionResult<PagedResponseDto<TaskDto>>> GetProjectTasks(int id, [FromQuery] QueryParametersDto query, CancellationToken ct)
        => Ok(await _service.GetProjectTasksAsync(id, query, ct));

    [HttpGet("{id:int}/kpis")]
    public async Task<ActionResult<ProjectKpisDto>> GetProjectKpis(int id, CancellationToken ct)
    {
        var kpis = await _service.GetProjectKpisAsync(id, ct);
        return kpis is null ? NotFound() : Ok(kpis);
    }

    [HttpGet("{id:int}/recent-documents")]
    public async Task<ActionResult<IReadOnlyList<RecentDocumentDto>>> GetRecentDocuments(int id, [FromQuery] int days = 30, [FromQuery] int limit = 10, CancellationToken ct = default)
        => Ok(await _service.GetRecentDocumentsAsync(id, days, limit, ct));

    [HttpGet("{id:int}/top-open-risks")]
    public async Task<ActionResult<IReadOnlyList<RiskDto>>> GetTopOpenRisks(int id, [FromQuery] int limit = 5, CancellationToken ct = default)
        => Ok(await _service.GetTopOpenRisksAsync(id, limit, ct));

    [HttpGet("{id:int}/upcoming-milestones")]
    public async Task<ActionResult<IReadOnlyList<MilestoneDto>>> GetUpcomingMilestones(int id, [FromQuery] int days = 30, CancellationToken ct = default)
        => Ok(await _service.GetUpcomingMilestonesAsync(id, days, ct));

    [HttpGet("{id:int}/rfis")]
    public async Task<ActionResult<IReadOnlyList<RfiSummaryDto>>> GetProjectRfis(int id, CancellationToken ct)
        => Ok(await _service.GetProjectRfisAsync(id, ct));

    [HttpGet("{id:int}/submittals")]
    public async Task<ActionResult<IReadOnlyList<SubmittalSummaryDto>>> GetProjectSubmittals(int id, CancellationToken ct)
        => Ok(await _service.GetProjectSubmittalsAsync(id, ct));

    [HttpGet("{id:int}/change-orders")]
    public async Task<ActionResult<IReadOnlyList<ChangeOrderSummaryDto>>> GetProjectChangeOrders(int id, CancellationToken ct)
        => Ok(await _service.GetProjectChangeOrdersAsync(id, ct));

    [HttpGet("locations")]
    public async Task<ActionResult<IReadOnlyList<MapLocationDto>>> GetProjectLocations(CancellationToken ct)
        => Ok(await _service.GetProjectLocationsAsync(ct));
}
