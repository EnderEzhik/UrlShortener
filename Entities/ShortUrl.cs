namespace Shortener.Entities;

public class ShortUrl
{
    public string OriginalUrl { get; set; } = null!;
    
    // Primary Key
    public string ShortCode { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ExpiresAt { get; set; }
}