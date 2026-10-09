namespace RetribusiPasar.Web.Models.Entities;

public class AuditLog : BaseEntity
{
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string UserName { get; set; } = "demo.user";
    public string Action { get; set; } = "";
    public string EntityName { get; set; } = "";
    public int EntityId { get; set; }
    public string Description { get; set; } = "";
}
