using IkProjesi.DTOs;

namespace IkProjesi.Services;

public interface IAuthService
{
    Task<TokenResponseDto?> RegisterAsync(RegisterDto dto);
    Task<TokenResponseDto?> LoginAsync(LoginDto dto);
}
