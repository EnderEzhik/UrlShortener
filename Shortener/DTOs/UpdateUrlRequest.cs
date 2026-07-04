namespace Shortener.DTOs;

public class UpdateUrlRequest
{
    public string? OriginalUrl { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}