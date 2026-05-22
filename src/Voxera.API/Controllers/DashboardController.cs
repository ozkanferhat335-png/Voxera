using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voxera.Application.Queries.Dashboard;

namespace Voxera.API.Controllers;

[ApiController]
[Route("api/v1/dashboard")]
[Authorize]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator) => _mediator = mediator;

    private Guid CompanyId => Guid.Parse(User.FindFirst("company_id")!.Value);

    /// <summary>Get real-time dashboard statistics</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDashboardStatsQuery(CompanyId), ct);
        return Ok(result);
    }

    /// <summary>Get active calls for the company</summary>
    [HttpGet("active-calls")]
    public async Task<IActionResult> GetActiveCalls(CancellationToken ct)
    {
        // TODO: Get from FreeSWITCH
        return Ok(new { activeCalls = new List<object>() });
    }
}
