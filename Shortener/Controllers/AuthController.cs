using Microsoft.AspNetCore.Mvc;
using Serilog.Context;
using Shortener.DTOs;
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
                _logger.Warning("Login is not valid");
            
                return Problem(
                    title: "Invalid login length",
                    detail: "Login length must be greater than or equal to 4",
                    statusCode: StatusCodes.Status400BadRequest,
                    type: "errors/invalid-login"
                );
            }
            if (requestData.Login.Length > 20)
            {
                _logger.Warning("Login is not valid");
            
                return Problem(
                    title: "Invalid login length",
                    detail: "Login length must be less than or equal to 20",
                    statusCode: StatusCodes.Status400BadRequest,
                    type: "errors/invalid-login"
                );
            }
            
            if (requestData.Password.Length < 8)
            {
                _logger.Warning("Password is not valid");
            
                return Problem(
                    title: "Invalid password length",
                    detail: "Password length must be greater than or equal to 8",
                    statusCode: StatusCodes.Status400BadRequest,
                    type: "errors/invalid-password"
                );
            }
            if (requestData.Password.Length > 64)
            {
                _logger.Warning("Password is not valid");
            
                return Problem(
                    title: "Invalid password length",
                    detail: "Password length must be less than or equal to 64",
                    statusCode: StatusCodes.Status400BadRequest,
                    type: "errors/invalid-password"
                );
            }
    
            try
            {
                var newUser = await _userService.CreateUser(requestData.Login, requestData.Password);
                
                _logger.Information("New user registered");

                var (token, expires) = _jwtService.GenerateJwtToken(newUser);
                return new JWTTokenResponse()
                {
                    Token = token,
                    ExpiresAt = expires
                };
            }
            catch (ArgumentException)
            {
                return Problem(
                    title: "Invalid login",
                    detail: "Login is already in use",
                    statusCode: StatusCodes.Status409Conflict,
                    type: "errors/invalid-login"
                );
            }
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<JWTTokenResponse>> Login(LoginRequest requestData)
    {
        using (LogContext.PushProperty("Login", requestData.Login))
        {
            _logger.Information("Authorization");
            
            if (requestData.Login.Length < 4)
            {
                _logger.Warning("Login is not valid");
            
                return Problem(
                    title: "Invalid login length",
                    detail: "Login length must be greater than or equal to 4",
                    statusCode: StatusCodes.Status400BadRequest,
                    type: "errors/invalid-login"
                );
            }
            if (requestData.Login.Length > 20)
            {
                _logger.Warning("Login is not valid");
            
                return Problem(
                    title: "Invalid login length",
                    detail: "Login length must be less than or equal to 20",
                    statusCode: StatusCodes.Status400BadRequest,
                    type: "errors/invalid-login"
                );
            }
            
            if (requestData.Password.Length < 8)
            {
                _logger.Warning("Password is not valid");
            
                return Problem(
                    title: "Invalid password length",
                    detail: "Password length must be greater than or equal to 8",
                    statusCode: StatusCodes.Status400BadRequest,
                    type: "errors/invalid-password"
                );
            }
            if (requestData.Password.Length > 64)
            {
                _logger.Warning("Password is not valid");
            
                return Problem(
                    title: "Invalid password length",
                    detail: "Password length must be less than or equal to 64",
                    statusCode: StatusCodes.Status400BadRequest,
                    type: "errors/invalid-password"
                );
            }
            
            var user = await _userService.GetUserByLogin(requestData.Login);
            if (user is null || user.Password != requestData.Password)
            {
                _logger.Warning("User not found or password is incorrect");
                
                return Problem(
                    title: "Could not log in",
                    detail: "Login or password is incorrect",
                    statusCode: StatusCodes.Status401Unauthorized,
                    type: "errors/invalid-login-data"
                );
            }
            var (token, expires) = _jwtService.GenerateJwtToken(user);
            
            _logger.Information("Successful authorization");
            
            return new JWTTokenResponse()
            {
                Token = token,
                ExpiresAt = expires
            };
        }
    }
}