namespace Shortener.DTOs;

public class RedirectAnalytics
{
    public string ShortCode { get; set; } = null!;
    public DateTimeOffset RedirectedAt { get; set; }
}
