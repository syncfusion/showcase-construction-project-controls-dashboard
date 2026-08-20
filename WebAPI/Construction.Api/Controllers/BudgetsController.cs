using Construction.Core.DTOs;
using Construction.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Construction.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BudgetsController : ControllerBase
{
    private readonly IBudgetService _service;

    public BudgetsController(IBudgetService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResponseDto<BudgetDto>>> GetBudgets([FromQuery] QueryParametersDto query, CancellationToken ct)
        => Ok(await _service.GetBudgetsAsync(query, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BudgetDto>> GetBudget(int id, CancellationToken ct)
    {
        var budget = await _service.GetBudgetByIdAsync(id, ct);
        return budget is null ? NotFound() : Ok(budget);
    }

    // NOTE: This showcase exposes a read-only API surface for public GitHub publication.
    // Create / Update / Delete actions are intentionally omitted to avoid anonymous
    // write/delete vectors (data tampering, DoS) on the demo database. The service-layer
    // write methods are likewise not part of the public surface. See README for details.
}
