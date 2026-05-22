using MediatR;
using Microsoft.EntityFrameworkCore;
using Voxera.Application.DTOs;
using Voxera.Application.Interfaces;
using Voxera.Domain.Enums;

namespace Voxera.Application.Queries.Calls;

public record GetCallLogsQuery(
    Guid CompanyId,
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    CallDirection? Direction = null,
    CallStatus? Status = null,
    DateTime? From = null,
    DateTime? To = null
) : IRequest<PagedResultDto<CallLogDto>>;

public class GetCallLogsQueryHandler : IRequestHandler<GetCallLogsQuery, PagedResultDto<CallLogDto>>
{
    private readonly IApplicationDbContext _db;

    public GetCallLogsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PagedResultDto<CallLogDto>> Handle(GetCallLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.CallLogs
            .Where(c => c.CompanyId == request.CompanyId && !c.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(c => c.CallerNumber!.Contains(request.Search) || c.CalleeNumber!.Contains(request.Search));

        if (request.Direction.HasValue)
            query = query.Where(c => c.Direction == request.Direction.Value);

        if (request.Status.HasValue)
            query = query.Where(c => c.Status == request.Status.Value);

        if (request.From.HasValue)
            query = query.Where(c => c.StartedAt >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(c => c.StartedAt <= request.To.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(c => c.StartedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CallLogDto(
                c.Id, c.CallId, c.CallerNumber, c.CallerName, c.CalleeNumber,
                c.Direction.ToString(), c.Status.ToString(),
                c.StartedAt, c.AnsweredAt, c.EndedAt, c.DurationSeconds,
                c.IsRecorded, c.RecordingPath, c.AiSummary, c.Sentiment.ToString()
            ))
            .ToListAsync(cancellationToken);

        return new PagedResultDto<CallLogDto>(items, total, request.Page, request.PageSize, (int)Math.Ceiling((double)total / request.PageSize));
    }
}
