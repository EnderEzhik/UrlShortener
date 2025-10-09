using System.Text.Json.Serialization;

namespace Shortener.Models;

public class CreateShortUrlRequest
{
    [JsonPropertyName("url")]
    public required string Url { get; set; }
    
    [JsonPropertyName("expiresAt")]
    public DateTime? ExpiresAt { get; set; }
}