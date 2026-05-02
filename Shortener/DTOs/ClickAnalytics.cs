namespace Shortener.DTOs;

public class ClickAnalytics
{
    public string ShortCode { get; set; } = null!;
    public DateTimeOffset Timestamp { get; set; }
    public string IpAddress { get; set; } = null!;
    public int? UserId { get; set; }
    public string? Referer { get; set; }
    public string Device { get; set; }
    public string Platform { get; set; }
    public string? CountryCode { get; set; }
    public string? AcceptLanguage { get; set; }
    public string? UserAgent { get; set; }
}

public enum PlatformType
{
    Windows,
    MacOS,
    Linux,
    Android,
    IOS
}