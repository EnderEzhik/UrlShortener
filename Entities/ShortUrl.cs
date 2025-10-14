namespace Shortener.Entities;

public class ShortUrl
{
    public string OriginalUrl { get; set; } = null!;
    
    // Primary Key
    public string ShortCode { get; set; } = null!;
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ExpiresAt { get; set; }
}