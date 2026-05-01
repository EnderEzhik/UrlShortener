using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Shortener.DTOs;

public class CreateShortUrlRequest
{
    [Required]
    [JsonPropertyName("url")]
    public string Url { get; set; }
    
    [JsonPropertyName("expiresAt")]
    public DateTimeOffset? ExpiresAt { get; set; }
}