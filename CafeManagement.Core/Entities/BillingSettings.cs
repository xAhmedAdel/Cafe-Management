namespace CafeManagement.Core.Entities;

public class BillingSettings : BaseEntity
{
    public decimal HourlyRate { get; set; } = 20.00m;
    public string Currency { get; set; } = "L.E";
    public string MinimumSessionDuration { get; set; } = "1 hour";
    public bool RoundUpToNearestHour { get; set; } = true;
    public string Description { get; set; } = "Default billing configuration";
    public bool IsActive { get; set; } = true;
}