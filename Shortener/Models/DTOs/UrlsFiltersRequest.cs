namespace Shortener.Models.DTOs;

public class UrlsFiltersRequest
{
    public string? containsSubstring { get; set; }
    public bool excludeExpiredUrls { get; set; } = true;
}