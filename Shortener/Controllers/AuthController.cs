using Microsoft.AspNetCore.Mvc;
using Serilog.Context;
using Shortener.DTOs;
using Shortener.Errors;
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
    public async Task<ActionResult<JWTTokenResponse>> RegisterAsync(UserCreateRequest requestData)
    {
        using var _ = LogContext.PushProperty("Login", requestData.Login);
        _logger.Information("Registering new user");

        if (requestData.Login.Length < 4 || requestData.Login.Length > 20)
        {
            _logger.Warning("Login length is incorrect");

            return this.Problem(ApiErrors.Auth.IncorrectLoginLength);
        }

        if (requestData.Password.Length < 8 || requestData.Password.Length > 64)
        {
            _logger.Warning("Password length is incorrect");

            return this.Problem(ApiErrors.Auth.IncorrectPasswordLength);
        }

        try
        {
            var newUser = await _userService.CreateUserAsync(requestData);

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
            return this.Problem(ApiErrors.Auth.LoginIsAlreadyInUse);
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<JWTTokenResponse>> LoginAsync(LoginRequest requestData)
    {
        using var _ = LogContext.PushProperty("Login", requestData.Login);
        _logger.Information("Authorization");

        if (requestData.Login.Length < 4 || requestData.Login.Length > 20)
        {
            _logger.Warning("Login length is incorrect");
            return this.Problem(ApiErrors.Auth.IncorrectLoginLength);
        }

        if (requestData.Password.Length < 8 || requestData.Password.Length > 64)
        {
            _logger.Warning("Password length is incorrect");
            return this.Problem(ApiErrors.Auth.IncorrectPasswordLength);
        }

        var user = await _userService.GetUserByLoginAsync(requestData.Login);
        if (user is null || user.Password != requestData.Password)
        {
            _logger.Warning("User not found or password is incorrect");
            return this.Problem(ApiErrors.Auth.IncorrectLoginOrPassword);
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