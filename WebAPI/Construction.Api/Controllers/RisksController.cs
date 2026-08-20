using Construction.Core.DTOs;
using Construction.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Construction.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RisksController : ControllerBase
{
    private readonly IRiskService _service;

    public RisksController(IRiskService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<PagedResponseDto<RiskDto>>> GetRisks([FromQuery] QueryParametersDto query, CancellationToken ct)
        => Ok(await _service.GetRisksAsync(query, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RiskDto>> GetRisk(int id, CancellationToken ct)
    {
        var risk = await _service.GetRiskByIdAsync(id, ct);
        return risk is null ? NotFound() : Ok(risk);
    }

    [HttpGet("kpis")]
    public async Task<ActionResult<RiskKpisDto>> GetKpis(CancellationToken ct)
        => Ok(await _service.GetKpisAsync(ct));

    [HttpGet("matrix")]
    public async Task<ActionResult<RiskMatrixDto>> GetMatrix(CancellationToken ct)
        => Ok(await _service.GetMatrixAsync(ct));

    // NOTE: Read-only public surface — see BudgetsController for rationale.
}
