using IkProjesi.DTOs;

namespace IkProjesi.Services;

public interface IAuthService
{
    Task<TokenResponseDto?> RegisterAsync(RegisterDto dto);
    Task<TokenResponseDto?> LoginAsync(LoginDto dto);
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto);
}
