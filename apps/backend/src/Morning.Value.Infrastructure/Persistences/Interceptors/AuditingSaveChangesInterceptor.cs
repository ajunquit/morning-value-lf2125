using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Morning.Value.Application.Common.Services;
using Morning.Value.Domain.Common.Entities;

namespace Morning.Value.Infrastructure.Persistences.Interceptors
{
    public class AuditingSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUserService _current;

        public AuditingSaveChangesInterceptor(ICurrentUserService current)
        {
            _current = current;
        }

        private static void ApplyAudit(DbContext context, string? currentUser)
        {
            var now = DateTime.UtcNow;

            foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    // Set Created*
                    entry.Property(e => e.CreatedAtUtc).CurrentValue = now;
                    entry.Property(e => e.CreatedBy).CurrentValue =
                        currentUser ?? entry.Property(e => e.CreatedBy).CurrentValue;
                }
                else if (entry.State == EntityState.Modified)
                {
                    // No tocar Created*
                    entry.Property(e => e.CreatedAtUtc).IsModified = false;
                    entry.Property(e => e.CreatedBy).IsModified = false;

                    // Set Modified*
                    entry.Property(e => e.ModifiedAtUtc).CurrentValue = now;
                    entry.Property(e => e.ModifiedBy).CurrentValue = currentUser;
                }
            }
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData, InterceptionResult<int> result)
        {
            if (eventData.Context is not null)
            {
                var user = _current.UserId ?? _current.Email ?? _current.UserName;
                ApplyAudit(eventData.Context, user);
            }
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null)
            {
                var user = _current.UserId ?? _current.Email ?? _current.UserName;
                ApplyAudit(eventData.Context, user);
            }
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
