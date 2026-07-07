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
    public async Task<ActionResult<JWTTokenResponse>> Register(UserCreateRequest requestData)
    {
        using var _ = LogContext.PushProperty("Login", requestData.Login);
        _logger.Information("Registering new user");

        if (requestData.Login.Length < 4 || requestData.Login.Length > 20)
        {
            _logger.Warning("Login length is incorrect");

            return this.Problem(ApiErrors.IncorrectLoginLength);
        }

        if (requestData.Password.Length < 8 || requestData.Password.Length > 64)
        {
            _logger.Warning("Password length is incorrect");

            return this.Problem(ApiErrors.IncorrectPasswordLength);
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

    [HttpPost("login")]
    public async Task<ActionResult<JWTTokenResponse>> Login(LoginRequest requestData)
    {
        using var _ = LogContext.PushProperty("Login", requestData.Login);
        _logger.Information("Authorization");

        if (requestData.Login.Length < 4 || requestData.Login.Length > 20)
        {
            _logger.Warning("Login length is incorrect");

            return this.Problem(ApiErrors.IncorrectLoginLength);
        }

        if (requestData.Password.Length < 8 || requestData.Password.Length > 64)
        {
            _logger.Warning("Password length is incorrect");

            return this.Problem(ApiErrors.IncorrectPasswordLength);
        }

        var user = await _userService.GetUserByLogin(requestData.Login);
        if (user is null || user.Password != requestData.Password)
        {
            _logger.Warning("User not found or password is incorrect");

            return this.Problem(ApiErrors.IncorrectLoginOrPassword);
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