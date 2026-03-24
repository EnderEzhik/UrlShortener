using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Shortener.Models.DTOs;
using Shortener.Services;

namespace Shortener.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly Serilog.ILogger logger = Log.ForContext<UsersController>();
    private readonly UserService _userService;
    
    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> GetMe()
    {
        logger.Information("Get request for get user me");
        
        var currentUserId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
        var parsedUserId = int.Parse(currentUserId);
        
        try
        {
            var currentUser = await _userService.GetUserById(parsedUserId);
            var userResponse = new UserResponse()
            {
                Username = currentUser.Login,
                RegistrationDate = currentUser.RegistrationAt
            };
            return userResponse;
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}