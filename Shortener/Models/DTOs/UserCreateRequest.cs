namespace Shortener.Models.DTOs;

public class UserCreateRequest
{
    public string Login { get; set; }
    public string Password { get; set; }
}