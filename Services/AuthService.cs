using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IkProjesi.DTOs;
using IkProjesi.Models;
using IkProjesi.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace IkProjesi.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository userRepo;
    private readonly IConfiguration config;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        userRepo = userRepository;
        config = configuration;
    }

    public async Task<TokenResponseDto?> RegisterAsync(RegisterDto dto)
    {
        User existingUser = await userRepo.GetByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            return null;
        }

        User newUser;

        if (dto.Rol == "Admin")
        {
            newUser = new Admin();
        }
        else if (dto.Rol == "IkYonetici")
        {
            newUser = new IkYonetici();
        }
        else if (dto.Rol == "Calisan")
        {
            Calisan newEmployee = new Calisan();
            newEmployee.PersonelId = dto.PersonelId.Value;
            newUser = newEmployee;
        }
        else
        {
            throw new ArgumentException("Geçersiz rol girildi.");
        }

        newUser.Email = dto.Email;
        newUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        newUser.Rol = dto.Rol;

        await userRepo.AddAsync(newUser);

        return BuildResponse(newUser);
    }

    public async Task<TokenResponseDto?> LoginAsync(LoginDto dto)
    {
        User foundUser = await userRepo.GetByEmailAsync(dto.Email);
        if (foundUser == null)
        {
            return null;
        }

        bool passwordCorrect = BCrypt.Net.BCrypt.Verify(dto.Password, foundUser.PasswordHash);
        if (passwordCorrect == false)
        {
            return null;
        }

        return BuildResponse(foundUser);
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        User user = await userRepo.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        bool currentPasswordCorrect = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash);
        if (currentPasswordCorrect == false)
        {
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await userRepo.UpdateAsync(user);

        return true;
    }

    private TokenResponseDto BuildResponse(User user)
    {
        string token = GenerateToken(user);

        UserDto userDto = new UserDto();
        userDto.Email = user.Email;
        userDto.Rol = user.Rol;
        if (user is Calisan)
        {
            Calisan employee = (Calisan)user;
            userDto.PersonelId = employee.PersonelId;
        }

        TokenResponseDto response = new TokenResponseDto();
        response.Token = token;
        response.User = userDto;
        return response;
    }

    private string GenerateToken(User user)
    {
        List<Claim> claimList = new List<Claim>();
        claimList.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
        claimList.Add(new Claim(ClaimTypes.Email, user.Email));
        claimList.Add(new Claim(ClaimTypes.Role, user.Rol));

        if (user is Calisan)
        {
            Calisan employee = (Calisan)user;
            claimList.Add(new Claim("PersonelId", employee.PersonelId.ToString()));
        }

        string secretKey = config["Jwt:Key"];
        byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);
        SymmetricSecurityKey securityKey = new SymmetricSecurityKey(keyBytes);
        SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claimList,
            expires: DateTime.UtcNow.AddHours(int.Parse(config["Jwt:ExpireHours"])),
            signingCredentials: signingCredentials
        );

        string tokenText = new JwtSecurityTokenHandler().WriteToken(token);
        return tokenText;
    }
}
