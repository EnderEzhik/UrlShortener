namespace Shortener.DTOs;

public class UrlsFiltersRequest
{
    public bool ExcludeExpiredUrls { get; set; } = true;
}