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
        TokenResponseDto? yanit = await authServis.RegisterAsync(dto);
        if (yanit == null)
        {
            return Conflict("Bu email zaten kayıtlı.");
        }

        return Ok(yanit);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        TokenResponseDto? yanit = await authServis.LoginAsync(dto);
        if (yanit == null)
        {
            return Unauthorized("Email veya şifre hatalı.");
        }

        return Ok(yanit);
    }
}
