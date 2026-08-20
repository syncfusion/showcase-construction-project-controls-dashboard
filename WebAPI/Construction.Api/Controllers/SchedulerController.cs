using Construction.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Construction.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SchedulerController : ControllerBase
{
    [HttpGet("appointments")]
    public ActionResult<List<AppointmentDto>> GetAppointments()
    {
        // Return empty array - scheduler appointments will come from inspections/milestones
        // when the frontend requests, we can populate from database
        return Ok(new List<AppointmentDto>());
    }
}
