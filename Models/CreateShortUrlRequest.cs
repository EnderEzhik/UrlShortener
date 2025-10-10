using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Shortener.Models;

public class CreateShortUrlRequest
{
    [Required]
    [JsonPropertyName("url")]
    public string Url { get; set; }
    
    [JsonPropertyName("expiresAt")]
    public DateTime? ExpiresAt { get; set; }
}