using Microsoft.AspNetCore.Mvc;

namespace Shortener.Controllers;

[ApiController]
public class RedirectorController : ControllerBase
{
    // Переадресация с сокращенного кода на оригинальную ссылку
    [HttpGet("{shortenCode}")]
    public async Task<IActionResult> Redirect(string shortenCode)
    {
        throw new NotImplementedException();
    }
}