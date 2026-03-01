using Microsoft.AspNetCore.Mvc;
using Shortener.Models.DTOs;
using Shortener.Services;

namespace Shortener.Controllers;

[ApiController]
[Route("api")]
public class AuthController : ControllerBase
{
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
        try
        {
            var newUser = await _userService.CreateUser(requestData.Login, requestData.Password);
            var token = _jwtService.GenerateJwtToken(newUser);
            return new JWTTokenResponse { Token = token };
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<JWTTokenResponse>> Login(LoginRequest loginData)
    {
        var user = await _userService.GetUser(loginData.Login);
        if (user is null || user.Password != loginData.Password)
        {
            return Unauthorized();
        }

        var token = _jwtService.GenerateJwtToken(user);

        var jwtResponse = new JWTTokenResponse()
        {
            Token = token
        };

        return jwtResponse;
    }
}