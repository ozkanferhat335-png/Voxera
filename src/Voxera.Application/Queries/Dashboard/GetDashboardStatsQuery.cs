using MediatR;
using Microsoft.EntityFrameworkCore;
using Voxera.Application.DTOs;
using Voxera.Application.Interfaces;
using Voxera.Domain.Enums;

namespace Voxera.Application.Queries.Dashboard;

public record GetDashboardStatsQuery(Guid CompanyId) : IRequest<DashboardStatsDto>;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICacheService _cache;

    public GetDashboardStatsQueryHandler(IApplicationDbContext db, ICacheService cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"dashboard:{request.CompanyId}";
        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var todayCalls = await _db.CallLogs
                .Where(c => c.CompanyId == request.CompanyId && c.StartedAt >= today && c.StartedAt < tomorrow)
                .ToListAsync(cancellationToken);

            var agents = await _db.SipAccounts
                .Where(s => s.CompanyId == request.CompanyId && s.Status == SipAccountStatus.Active)
                .ToListAsync(cancellationToken);

            var extensions = await _db.Extensions
                .CountAsync(e => e.CompanyId == request.CompanyId && e.Status == ExtensionStatus.Active, cancellationToken);

            var hourlyStats = Enumerable.Range(0, 24).Select(hour =>
            {
                var hourCalls = todayCalls.Where(c => c.StartedAt.Hour == hour).ToList();
                return new HourlyCallStatDto(
                    hour,
                    hourCalls.Count,
                    hourCalls.Count(c => c.Status == CallStatus.Completed),
                    hourCalls.Count(c => c.Status == CallStatus.Missed)
                );
            }).ToList();

            var avgDuration = todayCalls
                .Where(c => c.DurationSeconds.HasValue)
                .Select(c => c.DurationSeconds!.Value)
                .DefaultIfEmpty(0)
                .Average();

            return new DashboardStatsDto(
                TotalCallsToday: todayCalls.Count,
                ActiveCalls: todayCalls.Count(c => c.Status == CallStatus.Active),
                MissedCallsToday: todayCalls.Count(c => c.Status == CallStatus.Missed),
                TotalAgents: agents.Count,
                AvailableAgents: agents.Count(a => a.AgentStatus == AgentStatus.Available),
                BusyAgents: agents.Count(a => a.AgentStatus == AgentStatus.Busy),
                AverageCallDuration: Math.Round(avgDuration, 1),
                TotalExtensions: extensions,
                HourlyStats: hourlyStats
            );
        }, TimeSpan.FromSeconds(30), cancellationToken);
    }
}
