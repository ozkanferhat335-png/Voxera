using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Voxera.Application.Interfaces;
using Voxera.Domain.Entities;

namespace Voxera.API.Controllers;

[ApiController]
[Route("api/v1/queues")]
[Authorize]
[Produces("application/json")]
public class QueuesController : ControllerBase
{
    private readonly IApplicationDbContext _db;

    public QueuesController(IApplicationDbContext db) => _db = db;

    private Guid CompanyId => Guid.Parse(User.FindFirst("company_id")!.Value);

    /// <summary>List all call queues</summary>
    [HttpGet]
    public async Task<IActionResult> GetQueues(CancellationToken ct)
    {
        var queues = await _db.CallQueues
            .Where(q => q.CompanyId == CompanyId)
            .Include(q => q.Agents)
            .ThenInclude(a => a.Extension)
            .ToListAsync(ct);
        return Ok(queues);
    }

    /// <summary>Create a new call queue</summary>
    [HttpPost]
    [Authorize(Policy = "CompanyAdmin")]
    public async Task<IActionResult> CreateQueue([FromBody] CreateQueueRequest request, CancellationToken ct)
    {
        var queue = CallQueue.Create(CompanyId, request.Name, request.Extension);
        await _db.CallQueues.AddAsync(queue, ct);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetQueues), new { id = queue.Id }, new { id = queue.Id });
    }

    /// <summary>Add an agent to a queue</summary>
    [HttpPost("{queueId}/agents")]
    [Authorize(Policy = "CompanyAdmin")]
    public async Task<IActionResult> AddAgent(Guid queueId, [FromBody] AddQueueAgentRequest request, CancellationToken ct)
    {
        var queue = await _db.CallQueues.FirstOrDefaultAsync(q => q.Id == queueId && q.CompanyId == CompanyId, ct);
        if (queue is null) return NotFound();

        var agent = QueueAgent.Create(queueId, request.ExtensionId, request.Priority);
        await _db.QueueAgents.AddAsync(agent, ct);
        await _db.SaveChangesAsync(ct);
        return Ok(new { id = agent.Id });
    }

    /// <summary>Remove an agent from a queue</summary>
    [HttpDelete("{queueId}/agents/{agentId}")]
    [Authorize(Policy = "CompanyAdmin")]
    public async Task<IActionResult> RemoveAgent(Guid queueId, Guid agentId, CancellationToken ct)
    {
        var agent = await _db.QueueAgents.FirstOrDefaultAsync(a => a.Id == agentId && a.QueueId == queueId, ct);
        if (agent is null) return NotFound();
        agent.SoftDelete();
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}

public record CreateQueueRequest(string Name, string Extension, string? Description = null);
public record AddQueueAgentRequest(Guid ExtensionId, int Priority = 1);
