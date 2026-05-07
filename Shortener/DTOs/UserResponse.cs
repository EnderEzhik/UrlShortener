namespace Shortener.DTOs;

public class UserResponse
{
    public string Login { get; set; }
    public DateTimeOffset RegistrationAt { get; set; }
}