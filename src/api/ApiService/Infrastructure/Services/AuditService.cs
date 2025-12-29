using System.Text.Json;
using AspireAppTemplate.ApiService.Data;
using AspireAppTemplate.ApiService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AspireAppTemplate.ApiService.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        AppDbContext dbContext,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditService> logger)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task LogAsync(string action, string entityName, string entityId, object? oldValues, object? newValues, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
            var userName = "System";

            if (!string.IsNullOrEmpty(userId))
            {
                // Try to resolve user name from local DB
                // We use a fresh scope or check if the current context has it tracked? 
                // Since this is a Scoped service, we can use the same DbContext.
                // However, to be safe and avoid tracking issues if we are in a middle of a transaction or something,
                // we'll just query it.
                if (Guid.TryParse(userId, out var userGuid))
                {
                    var user = await _dbContext.Users.FindAsync(new object[] { userGuid }, cancellationToken);
                    if (user != null)
                    {
                        // Prefer Real Name, fallback to Username
                        if (!string.IsNullOrEmpty(user.FirstName) || !string.IsNullOrEmpty(user.LastName))
                        {
                            userName = $"{user.LastName} {user.FirstName}".Trim();
                        }
                        else
                        {
                            userName = user.Username;
                        }
                    }
                    else
                    {
                        userName = "Unknown User";
                    }
                }
            }
            else
            {
                userId = "system";
            }

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                UserName = userName,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Timestamp = DateTimeOffset.UtcNow,
                OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null
            };

            _dbContext.AuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Audit logging should not break the main flow, but we should log the error
            _logger.LogError(ex, "Failed to create audit log for {EntityName} {EntityId}", entityName, entityId);
        }
    }
}
