namespace Shortener.DTOs;

public class JWTTokenResponse
{
    public string Token { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
}