using IkProjesi.DTOs;
using IkProjesi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IkProjesi.Controllers;

[ApiController]
[Route("Auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService authService;

    public AuthController(IAuthService service)
    {
        authService = service;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        TokenResponseDto? response = await authService.RegisterAsync(dto);
        if (response == null)
        {
            return Conflict("Bu email zaten kayıtlı.");
        }

        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        TokenResponseDto? response = await authService.LoginAsync(dto);
        if (response == null)
        {
            return Unauthorized("Email veya şifre hatalı.");
        }

        return Ok(response);
    }

    [HttpPut("changePassword")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        string userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdText == null)
        {
            return Unauthorized();
        }

        int userId = int.Parse(userIdText);
        bool success = await authService.ChangePasswordAsync(userId, dto);
        if (success == false)
        {
            return BadRequest("Mevcut şifre hatalı.");
        }

        return NoContent();
    }
}
