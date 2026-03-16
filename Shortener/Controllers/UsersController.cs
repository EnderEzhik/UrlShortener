using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shortener.Models.DTOs;
using Shortener.Services;

namespace Shortener.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    
    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> GetMe()
    {
        var currentUserLogin = HttpContext.User.Identity.Name;
        var currentUser = await _userService.GetUser(currentUserLogin);
        var userResponse = new UserResponse()
        {
            Username = currentUser.Login,
            RegistrationDate = currentUser.RegistrationAt
        };
        return userResponse;
    }
}