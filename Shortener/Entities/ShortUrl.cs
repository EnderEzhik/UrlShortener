namespace Shortener.Entities;

public class ShortUrl
{
    /// <summary>
    /// Primary Key
    /// </summary>
    public string ShortCode { get; set; } = null!;
    public string OriginalUrl { get; set; } = null!;
    
    /// <summary>
    /// Owner user id
    /// </summary>
    public int? OwnerId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ExpiresAt { get; set; }
    
    public User? User { get; set; }
}