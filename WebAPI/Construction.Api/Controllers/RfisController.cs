using Construction.Core.DTOs;
using Construction.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Construction.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RfisController : ControllerBase
{
    private readonly IRfiService _service;

    public RfisController(IRfiService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResponseDto<RfiDto>>> GetRfis([FromQuery] QueryParametersDto query, CancellationToken ct)
        => Ok(await _service.GetRfisAsync(query, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RfiDto>> GetRfi(int id, CancellationToken ct)
    {
        var rfi = await _service.GetRfiByIdAsync(id, ct);
        return rfi is null ? NotFound() : Ok(rfi);
    }

    // NOTE: Read-only public surface — see BudgetsController for rationale.
}
