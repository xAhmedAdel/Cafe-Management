using CafeManagement.Core.Enums;

namespace CafeManagement.Core.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public decimal Balance { get; set; } = 0.00m;

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
    public virtual ICollection<UsageLog> UsageLogs { get; set; } = new List<UsageLog>();
}