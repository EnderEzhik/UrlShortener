namespace Shortener.Entities;

public class Redirect
{
    public int Id { get; set; }
    public string ShortCode { get; set; } = null!;
    public DateTimeOffset RedirectedAt { get; set; }
}