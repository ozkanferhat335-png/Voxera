using Microsoft.EntityFrameworkCore;
using Voxera.Application.Interfaces;
using Voxera.Domain.Entities;

namespace Voxera.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Extension> Extensions => Set<Extension>();
    public DbSet<SipAccount> SipAccounts => Set<SipAccount>();
    public DbSet<CallLog> CallLogs => Set<CallLog>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<IvrMenu> IvrMenus => Set<IvrMenu>();
    public DbSet<IvrOption> IvrOptions => Set<IvrOption>();
    public DbSet<IvrSchedule> IvrSchedules => Set<IvrSchedule>();
    public DbSet<CallQueue> CallQueues => Set<CallQueue>();
    public DbSet<QueueAgent> QueueAgents => Set<QueueAgent>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filters for soft delete
        modelBuilder.Entity<Company>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Extension>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SipAccount>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<CallLog>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ApiKey>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.MarkAsUpdated();
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch domain events
        var entities = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        foreach (var entity in entities)
            entity.ClearDomainEvents();

        return result;
    }
}
