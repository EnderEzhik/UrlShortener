namespace Shortener.DTOs;

public class ShortUrlResponse
{
    public string Url { get; set; }
    public string ShortCode { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}