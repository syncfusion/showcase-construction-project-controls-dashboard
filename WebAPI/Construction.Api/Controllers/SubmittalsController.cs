using Construction.Core.DTOs;
using Construction.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Construction.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubmittalsController : ControllerBase
{
    private readonly ISubmittalService _service;

    public SubmittalsController(ISubmittalService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResponseDto<SubmittalDto>>> GetSubmittals([FromQuery] QueryParametersDto query, CancellationToken ct)
        => Ok(await _service.GetSubmittalsAsync(query, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SubmittalDto>> GetSubmittal(int id, CancellationToken ct)
    {
        var submittal = await _service.GetSubmittalByIdAsync(id, ct);
        return submittal is null ? NotFound() : Ok(submittal);
    }

    // NOTE: Read-only public surface — see BudgetsController for rationale.
}
