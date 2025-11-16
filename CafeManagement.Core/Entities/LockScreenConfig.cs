namespace CafeManagement.Core.Entities;

public class LockScreenConfig : BaseEntity
{
    public int ClientId { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string BackgroundColor { get; set; } = "#000000";
    public string TextColor { get; set; } = "#FFFFFF";
    public string Message { get; set; } = string.Empty;
    public bool ShowTimeRemaining { get; set; } = true;
    public string CustomCSS { get; set; } = string.Empty;

    public virtual Client Client { get; set; } = null!;
}