namespace AspireAppTemplate.ApiService.Infrastructure.Services;

public interface IAuditService
{
    Task LogAsync(string action, string entityName, string entityId, object? oldValues, object? newValues, CancellationToken cancellationToken = default);
}
