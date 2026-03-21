using Microsoft.AspNetCore.Mvc;
using Serilog;
using Shortener.Models.DTOs;
using Shortener.Services;

namespace Shortener.Controllers;

[ApiController]
[Route("api")]
public class AuthController : ControllerBase
{
    private readonly Serilog.ILogger logger = Log.ForContext<AuthController>();
    private readonly UserService _userService;
    private readonly JwtService _jwtService;
    
    public AuthController(UserService  userService, JwtService jwtService)
    {
        _userService =  userService;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<JWTTokenResponse>> Register(UserCreateRequest requestData)
    {
        logger.Information("Post request for register user");

        if (requestData.Login.Length < 4)
        {
            logger.Warning("Password length is less than 4");
            return BadRequest(new
            {
                message = "Login must contain at least 4 characters"
            });
        }
        if (requestData.Password.Length < 8)
        {
            logger.Warning("Password length is less than 8");
            return BadRequest(new
            {
                message = "Password must contain at least 8 characters"
            });
        }
        
        try
        {
            var newUser = await _userService.CreateUser(requestData.Login, requestData.Password);
            logger.Information("New user created");

            logger.Information("Generating jwt token");
            var token = _jwtService.GenerateJwtToken(newUser);
            logger.Information("Successfully generated jwt token");

            return new JWTTokenResponse { Token = token };
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<JWTTokenResponse>> Login(LoginRequest loginData)
    {
        logger.Information("Post request for login");
        try
        {
            var user = await _userService.GetUserByLogin(loginData.Login);
            if (user is null || user.Password != loginData.Password)
            {
                return Unauthorized();
            }

            logger.Information("Generating jwt token");
            var token = _jwtService.GenerateJwtToken(user);
            logger.Information("Successfully generated jwt token");
            return new JWTTokenResponse { Token = token };
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}