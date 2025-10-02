using Microsoft.AspNetCore.Mvc;
using Shortener.Models;

namespace Shortener.Controllers;

[ApiController]
[Route("[controller]")]
public class ShortenController : ControllerBase
{
    // Создать сокращенный код для ссылки
    [HttpPost]
    public async Task<IActionResult> ShortenUrl(ShorteningUrl requestData)
    {
        throw new NotImplementedException();
    }
}