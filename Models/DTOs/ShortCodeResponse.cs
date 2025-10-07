using System.Text.Json.Serialization;

namespace Shortener.Models.DTOs;

public class ShortCodeResponse
{
    [JsonPropertyName("shortCode")]
    public string ShortCode { get; set; }
    
    [JsonPropertyName("expiresAt")]
    public DateTime? ExpiresAt { get; set; }
}