using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Voxera.API.Hubs;

/// <summary>
/// SignalR hub for real-time call events.
/// Clients subscribe to their company's call events.
/// </summary>
[Authorize]
public class CallHub : Hub
{
    private readonly ILogger<CallHub> _logger;

    public CallHub(ILogger<CallHub> logger) => _logger = logger;

    public override async Task OnConnectedAsync()
    {
        var companyId = Context.User?.FindFirst("company_id")?.Value;
        if (companyId is not null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"company:{companyId}");
            _logger.LogInformation("Client {ConnectionId} joined company group {CompanyId}", Context.ConnectionId, companyId);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var companyId = Context.User?.FindFirst("company_id")?.Value;
        if (companyId is not null)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"company:{companyId}");
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>Agent updates their status</summary>
    public async Task UpdateAgentStatus(string status)
    {
        var companyId = Context.User?.FindFirst("company_id")?.Value;
        var userId = Context.User?.FindFirst("sub")?.Value;
        if (companyId is null) return;

        await Clients.Group($"company:{companyId}").SendAsync("AgentStatusChanged", new
        {
            userId,
            status,
            timestamp = DateTime.UtcNow
        });
    }
}

/// <summary>
/// SignalR hub for real-time dashboard updates.
/// </summary>
[Authorize]
public class DashboardHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var companyId = Context.User?.FindFirst("company_id")?.Value;
        if (companyId is not null)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"dashboard:{companyId}");
        await base.OnConnectedAsync();
    }
}
