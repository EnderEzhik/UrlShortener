namespace Shortener.Models.DTOs;

public class UrlsFiltersRequest
{
    public bool ExcludeExpiredUrls { get; set; } = true;
}