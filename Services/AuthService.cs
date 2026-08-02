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
    private readonly IPersonnelRepository personnelRepo;
    private readonly IConfiguration config;

    public AuthService(IUserRepository userRepository, IPersonnelRepository personnelRepository, IConfiguration configuration)
    {
        userRepo = userRepository;
        personnelRepo = personnelRepository;
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

    public async Task<(TokenResponseDto? response, string message)> LoginAsync(LoginDto dto)
    {
        User foundUser = await userRepo.GetByEmailAsync(dto.Email);
        if (foundUser == null)
        {
            return (null, "Email veya şifre hatalı.");
        }

        bool passwordCorrect = BCrypt.Net.BCrypt.Verify(dto.Password, foundUser.PasswordHash);
        if (passwordCorrect == false)
        {
            return (null, "Email veya şifre hatalı.");
        }

        // İşten ayrılmış personel sisteme giriş yapamaz.
        if (foundUser is Calisan)
        {
            Calisan employee = (Calisan)foundUser;
            Personel personnel = await personnelRepo.GetByIdAsync(employee.PersonelId);

            if (personnel != null && personnel.AktifMi == false)
            {
                return (null, "Bu hesap aktif değil. İnsan Kaynakları ile görüşün.");
            }
        }

        return (BuildResponse(foundUser), "Giriş başarılı.");
    }

    // İK, şifresini unutan kullanıcının şifresini varsayılana döndürür.
    // Kullanıcı ilk girişinde şifre değiştirmeye yönlendirilsin diye
    // IsFirstLogin tekrar true yapılır.
    public async Task<(bool success, string message)> ResetPasswordAsync(ResetPasswordDto dto)
    {
        User user = await userRepo.GetByEmailAsync(dto.Email);
        if (user == null)
        {
            return (false, "Bu email ile kayıtlı kullanıcı bulunamadı.");
        }

        string defaultPassword = await BuildDefaultPasswordAsync(user);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword);
        user.IsFirstLogin = true;
        await userRepo.UpdateAsync(user);

        return (true, "Şifre sıfırlandı. Yeni şifre: " + defaultPassword);
    }

    private async Task<string> BuildDefaultPasswordAsync(User user)
    {
        string baseName;

        if (user is Calisan)
        {
            Calisan employee = (Calisan)user;
            Personel personnel = await personnelRepo.GetByIdAsync(employee.PersonelId);

            if (personnel != null)
            {
                baseName = personnel.Ad;
            }
            else
            {
                baseName = user.Email.Split('@')[0];
            }
        }
        else
        {
            baseName = user.Email.Split('@')[0];
        }

        string trimmed = baseName.Trim();
        string capitalized = char.ToUpper(trimmed[0]) + trimmed.Substring(1).ToLower();
        return capitalized + "123!";
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
        user.IsFirstLogin = false;
        await userRepo.UpdateAsync(user);

        return true;
    }

    private TokenResponseDto BuildResponse(User user)
    {
        string token = GenerateToken(user);

        UserDto userDto = new UserDto();
        userDto.Email = user.Email;
        userDto.Rol = user.Rol;
        userDto.IsFirstLogin = user.IsFirstLogin;
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
