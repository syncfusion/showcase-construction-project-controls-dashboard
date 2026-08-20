using Construction.Core.DTOs;
using Construction.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Construction.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _service;

    public TasksController(ITaskService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResponseDto<TaskDto>>> GetTasks([FromQuery] QueryParametersDto query, CancellationToken ct)
        => Ok(await _service.GetTasksAsync(query, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskDto>> GetTask(int id, CancellationToken ct)
    {
        var task = await _service.GetTaskByIdAsync(id, ct);
        return task is null ? NotFound() : Ok(task);
    }

    // NOTE: Read-only public surface — see BudgetsController for rationale.
}
