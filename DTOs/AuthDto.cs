namespace IkProjesi.DTOs;

public class RegisterDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public int? PersonelId { get; set; }
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UserDto
{
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public int? PersonelId { get; set; }
}

public class TokenResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = new UserDto();
}

public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
