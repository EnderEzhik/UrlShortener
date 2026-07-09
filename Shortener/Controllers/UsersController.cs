using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shortener.DTOs;
using Shortener.Services;

namespace Shortener.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly Serilog.ILogger _logger;
    private readonly UserService _userService;
    
    public UsersController(UserService userService)
    {
        _userService = userService;
        _logger = Serilog.Log.ForContext<UsersController>();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> GetMe()
    {
        _logger.Information("Getting user");
        
        var rawUserId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)!.Value;
        var userId = int.Parse(rawUserId);
        
        var currentUser = await _userService.GetUserByIdAsync(userId);
        
        _logger.Information("Got user");
        
        return new UserResponse()
        {
            Login = currentUser.Login,
            RegistrationAt = currentUser.RegistrationAt
        };
    }
}