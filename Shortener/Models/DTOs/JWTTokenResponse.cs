namespace Shortener.Models.DTOs;

public class JWTTokenResponse
{
    public string Token { get; init; }
    public DateTimeOffset Expires { get; init; }
}