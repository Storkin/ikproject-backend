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
    private readonly IUserRepository kullaniciDepo;
    private readonly IConfiguration ayarlar;

    public AuthService(IUserRepository userRepository, IConfiguration config)
    {
        kullaniciDepo = userRepository;
        ayarlar = config;
    }

    public async Task<TokenResponseDto?> RegisterAsync(RegisterDto dto)
    {
        User mevcutKullanici = await kullaniciDepo.GetByEmailAsync(dto.Email);
        if (mevcutKullanici != null)
        {
            return null;
        }

        User yeniKullanici;

        if (dto.Rol == "Admin")
        {
            yeniKullanici = new Admin();
        }
        else if (dto.Rol == "IkYonetici")
        {
            yeniKullanici = new IkYonetici();
        }
        else if (dto.Rol == "Calisan")
        {
            Calisan yeniCalisan = new Calisan();
            yeniCalisan.PersonelId = dto.PersonelId.Value;
            yeniKullanici = yeniCalisan;
        }
        else
        {
            throw new ArgumentException("Geçersiz rol girildi.");
        }

        yeniKullanici.Email = dto.Email;
        yeniKullanici.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        yeniKullanici.Rol = dto.Rol;

        await kullaniciDepo.AddAsync(yeniKullanici);

        return YanitYap(yeniKullanici);
    }

    public async Task<TokenResponseDto?> LoginAsync(LoginDto dto)
    {

        User bulunanKullanici = await kullaniciDepo.GetByEmailAsync(dto.Email);
        if (bulunanKullanici == null)
        {
            return null;
        }

        bool sifreDogruMu = BCrypt.Net.BCrypt.Verify(dto.Password, bulunanKullanici.PasswordHash);
        if (sifreDogruMu == false)
        {
            return null;
        }

        return YanitYap(bulunanKullanici);
    }

    private TokenResponseDto YanitYap(User kullanici)
    {
        string token = TokenUret(kullanici);

        UserDto userDto = new UserDto();
        userDto.Email = kullanici.Email;
        userDto.Rol = kullanici.Rol;
        if (kullanici is Calisan)
        {
            Calisan calisan = (Calisan)kullanici;
            userDto.PersonelId = calisan.PersonelId;
        }

        TokenResponseDto yanit = new TokenResponseDto();
        yanit.Token = token;
        yanit.User = userDto;
        return yanit;
    }

    private string TokenUret(User kullanici)
    {
        List<Claim> claimListesi = new List<Claim>();
        claimListesi.Add(new Claim(ClaimTypes.NameIdentifier, kullanici.Id.ToString()));
        claimListesi.Add(new Claim(ClaimTypes.Email, kullanici.Email));
        claimListesi.Add(new Claim(ClaimTypes.Role, kullanici.Rol));

        if (kullanici is Calisan)
        {
            Calisan calisan = (Calisan)kullanici;
            claimListesi.Add(new Claim("PersonelId", calisan.PersonelId.ToString()));
        }

        string gizliAnahtar = ayarlar["Jwt:Key"];
        byte[] anahtarBytes = Encoding.UTF8.GetBytes(gizliAnahtar);
        SymmetricSecurityKey guvenlikAnahtari = new SymmetricSecurityKey(anahtarBytes);
        SigningCredentials imzaAyarlari = new SigningCredentials(guvenlikAnahtari, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: ayarlar["Jwt:Issuer"],
            audience: ayarlar["Jwt:Audience"],
            claims: claimListesi,
            expires: DateTime.UtcNow.AddHours(int.Parse(ayarlar["Jwt:ExpireHours"])),
            signingCredentials: imzaAyarlari
        );

        string tokenMetni = new JwtSecurityTokenHandler().WriteToken(token);
        return tokenMetni;
    }
}
