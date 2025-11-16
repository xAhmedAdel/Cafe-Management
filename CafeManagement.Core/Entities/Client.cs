using CafeManagement.Core.Enums;

namespace CafeManagement.Core.Entities;

public class Client : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string IPAddress { get; set; } = string.Empty;
    public string MACAddress { get; set; } = string.Empty;
    public ClientStatus Status { get; set; } = ClientStatus.Offline;
    public string Configuration { get; set; } = string.Empty;
    public DateTime? LastSeen { get; set; }
    public int? CurrentSessionId { get; set; }

    public virtual Session? CurrentSession { get; set; }
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
    public virtual LockScreenConfig LockScreenConfig { get; set; } = null!;
    public virtual ICollection<UsageLog> UsageLogs { get; set; } = new List<UsageLog>();
}