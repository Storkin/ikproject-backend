using IkProjesi.DTOs;
using IkProjesi.Services;
using Microsoft.AspNetCore.Mvc;

namespace IkProjesi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService authServis;

    public AuthController(IAuthService service)
    {
        authServis = service;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        string uretılenToken = await authServis.RegisterAsync(dto);
        if (uretılenToken == null)
        {
            return Conflict("Bu email zaten kayıtlı.");
        }

        TokenResponseDto yanit = new TokenResponseDto();
        yanit.Token = uretılenToken;
        return Ok(yanit);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        string uretılenToken = await authServis.LoginAsync(dto);
        if (uretılenToken == null)
        {
            return Unauthorized("Email veya şifre hatalı.");
        }

        TokenResponseDto yanit = new TokenResponseDto();
        yanit.Token = uretılenToken;
        return Ok(yanit);
    }
}
