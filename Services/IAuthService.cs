using IkProjesi.DTOs;

namespace IkProjesi.Services;

public interface IAuthService
{
    Task<TokenResponseDto?> RegisterAsync(RegisterDto dto);
    Task<(TokenResponseDto? response, string message)> LoginAsync(LoginDto dto);
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto);
    Task<(bool success, string message)> ResetPasswordAsync(ResetPasswordDto dto);
}
