namespace Shortener.Entities;

public class Url
{
    public int Id { get; set; }
    public string OriginalUrl { get; set; }
    public string ShortenCode { get; set; }
}