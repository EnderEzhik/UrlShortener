namespace Shortener.DTOs;

public class UpdateUrlRequest
{
    public string Url { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}