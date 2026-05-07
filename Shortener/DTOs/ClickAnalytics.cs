namespace Shortener.DTOs;

public class ClickAnalytics
{
    public string ShortCode { get; set; } = null!;
    public DateTimeOffset RedirectAt { get; set; }
    public string IpAddress { get; set; } = null!;
    public int? UserId { get; set; }
    public string? Referer { get; set; }
}
