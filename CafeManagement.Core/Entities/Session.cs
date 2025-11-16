using CafeManagement.Core.Enums;

namespace CafeManagement.Core.Entities;

public class Session : BaseEntity
{
    public int ClientId { get; set; }
    public int? UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public decimal HourlyRate { get; set; } = 2.00m;
    public decimal TotalAmount { get; set; } = 0.00m;
    public SessionStatus Status { get; set; } = SessionStatus.Active;
    public string Notes { get; set; } = string.Empty;

    public virtual Client Client { get; set; } = null!;
    public virtual User? User { get; set; }
}