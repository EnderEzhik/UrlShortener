using System.Text.Json.Serialization;

namespace Shortener.Models;

public class ShorteningUrl
{
    [JsonPropertyName("url")]
    public required string Url { get; set; }
}