using Voxera.Domain.Entities;
using Voxera.Infrastructure.Persistence;

namespace Voxera.API.Middleware;

/// <summary>
/// Middleware that logs all write operations (POST, PUT, PATCH, DELETE) to the audit log.
/// </summary>
public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly HashSet<string> AuditedMethods = new() { "POST", "PUT", "PATCH", "DELETE" };

    public AuditMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
    {
        await _next(context);

        if (!AuditedMethods.Contains(context.Request.Method)) return;
        if (context.Response.StatusCode >= 500) return;

        try
        {
            Guid? userId = null;
            Guid? companyId = null;

            if (context.User.Identity?.IsAuthenticated == true)
            {
                Guid.TryParse(context.User.FindFirst("sub")?.Value, out var uid);
                Guid.TryParse(context.User.FindFirst("company_id")?.Value, out var cid);
                userId = uid == Guid.Empty ? null : uid;
                companyId = cid == Guid.Empty ? null : cid;
            }

            var auditLog = AuditLog.Create(companyId, userId, context.Request.Method, context.Request.Path);
            auditLog.SetRequest(context.Connection.RemoteIpAddress?.ToString(), context.Request.Headers.UserAgent.ToString());

            if (context.Response.StatusCode >= 400)
                auditLog.MarkFailed($"HTTP {context.Response.StatusCode}");

            await db.AuditLogs.AddAsync(auditLog);
            await db.SaveChangesAsync();
        }
        catch
        {
            // Audit logging should never break the request
        }
    }
}
