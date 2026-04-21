using Microsoft.AspNetCore.Mvc;
using Serilog.Context;
using Shortener.Models.DTOs;
using Shortener.Services;

namespace Shortener.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly Serilog.ILogger _logger;
    private readonly UserService _userService;
    private readonly JwtService _jwtService;
    
    public AuthController(UserService  userService, JwtService jwtService)
    {
        _userService =  userService;
        _jwtService = jwtService;
        _logger = Serilog.Log.ForContext<AuthController>();
    }

    [HttpPost("register")]
    public async Task<ActionResult<JWTTokenResponse>> Register(UserCreateRequest requestData)
    {
        using (LogContext.PushProperty("Login", requestData.Login))
        {
            _logger.Information("Registering new user");
            
            if (requestData.Login.Length < 4)
            {
                _logger.Warning("Login length is less than 4");
            
                return BadRequest(new
                {
                    message = "Login must contain at least 4 characters"
                });
            }
        
            if (requestData.Password.Length < 8)
            {
                _logger.Warning("Password length is less than 8");
            
                return BadRequest(new
                {
                    message = "Password must contain at least 8 characters"
                });
            }
    
            try
            {
                var newUser = await _userService.CreateUser(requestData.Login, requestData.Password);
                
                _logger.Information("New user registered");

                var (token, expires) = _jwtService.GenerateJwtToken(newUser);
    
                _logger.Debug("Token generated");
                
                return new JWTTokenResponse { Token = token, Expires = expires };
            }
            catch (ArgumentException)
            {
                return Conflict();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while registering user");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<JWTTokenResponse>> Login(LoginRequest loginData)
    {
        using (LogContext.PushProperty("Login", loginData.Login))
        {
            _logger.Information("Authorization");
            
            try
            {
                var user = await _userService.GetUserByLogin(loginData.Login);
                if (user is null || user.Password != loginData.Password)
                {
                    _logger.Warning("User not found or password is incorrect");
                    return Unauthorized();
                }
                var (token, expires) = _jwtService.GenerateJwtToken(user);
                
                _logger.Information("Successful authorization");
                
                return new JWTTokenResponse { Token = token, Expires = expires };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while authorization");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}