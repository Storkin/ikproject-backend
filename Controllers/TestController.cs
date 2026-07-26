using Microsoft.AspNetCore.Mvc;

namespace IkProjesi.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("API çalışıyor!!!");
    }

    [HttpGet("saat")]
    public IActionResult SaatKac()
    {
        return Ok("Sunucu saati: " + DateTime.Now.ToString());
    }
}
