namespace Shortener.Models.DTOs;

public class UrlsFiltersRequest
{
    public string? ContainsSubstring { get; set; } = null;
    public bool ExcludeExpiredUrls { get; set; } = true;
}