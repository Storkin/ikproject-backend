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

    // Hesap olusturma yetkisi sadece IK'da. Aksi halde disaridan
    // herkes kendine IkYonetici hesabi acip tum maaslari gorebilirdi.
    [HttpPost("register")]
    [Authorize(Roles = "IkYonetici")]
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
        (TokenResponseDto? response, string message) result = await authService.LoginAsync(dto);
        if (result.response == null)
        {
            return Unauthorized(result.message);
        }

        return Ok(result.response);
    }

    [HttpPut("resetPassword")]
    [Authorize(Roles = "IkYonetici")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        (bool success, string message) result = await authService.ResetPasswordAsync(dto);
        if (result.success == false)
        {
            return BadRequest(result.message);
        }

        return Ok(result.message);
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
