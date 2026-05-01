using System.Text.Json.Serialization;

namespace Shortener.DTOs;

public class ShortCodeResponse
{
    [JsonPropertyName("shortCode")]
    public string ShortCode { get; set; }
    
    [JsonPropertyName("expiresAt")]
    public DateTimeOffset? ExpiresAt { get; set; }
}