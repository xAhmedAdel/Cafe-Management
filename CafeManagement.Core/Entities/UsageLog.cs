namespace CafeManagement.Core.Entities;

public class UsageLog : BaseEntity
{
    public int ClientId { get; set; }
    public int? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public virtual Client Client { get; set; } = null!;
    public virtual User? User { get; set; }
}