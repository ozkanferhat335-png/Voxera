using Microsoft.EntityFrameworkCore;
using Voxera.Domain.Entities;

namespace Voxera.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Company> Companies { get; }
    DbSet<User> Users { get; }
    DbSet<Extension> Extensions { get; }
    DbSet<SipAccount> SipAccounts { get; }
    DbSet<CallLog> CallLogs { get; }
    DbSet<ApiKey> ApiKeys { get; }
    DbSet<IvrMenu> IvrMenus { get; }
    DbSet<IvrOption> IvrOptions { get; }
    DbSet<IvrSchedule> IvrSchedules { get; }
    DbSet<CallQueue> CallQueues { get; }
    DbSet<QueueAgent> QueueAgents { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<InvoiceItem> InvoiceItems { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
