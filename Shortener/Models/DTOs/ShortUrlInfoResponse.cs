namespace Shortener.Models.DTOs;

public class ShortUrlInfoResponse
{
    public string OriginalUrl { get; set; }
    public string ShortCode { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}