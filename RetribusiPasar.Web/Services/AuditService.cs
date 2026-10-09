using RetribusiPasar.Web.Infrastructure.Repositories;
using RetribusiPasar.Web.Models.Entities;

namespace RetribusiPasar.Web.Services;

public class AuditService
{
    private readonly IRepository<AuditLog> _repository;
    private readonly IHttpContextAccessor _http;
    public AuditService(IRepository<AuditLog> repository, IHttpContextAccessor http){_repository=repository;_http=http;}

    public async Task LogAsync(string action, string entityName, int entityId, string description)
    {
        await _repository.AddAsync(new AuditLog
        {
            Timestamp = DateTime.Now,
            UserName = _http.HttpContext?.User?.Identity?.Name ?? "system",
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Description = description
        });
    }
}
