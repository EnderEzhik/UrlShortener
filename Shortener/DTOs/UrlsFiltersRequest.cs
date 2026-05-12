namespace Shortener.DTOs;

public class UrlsFiltersRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public bool ExcludeExpiredUrls { get; set; } = true;
}