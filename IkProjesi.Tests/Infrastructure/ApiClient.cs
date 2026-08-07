using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace IkProjesi.Tests.Infrastructure;

/// <summary>
/// Testlerin HTTP ayrintilariyla ugrasmamasi icin ince bir sarmalayici.
/// Token yonetimi, JSON ayarlari ve sik kullanilan cagrilar burada toplanir.
/// </summary>
public class ApiClient
{
    private readonly HttpClient http;

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(HttpClient httpClient)
    {
        http = httpClient;
    }

    public HttpClient Raw => http;

    public async Task<ApiClient> LoginAsAsync(string email, string password)
    {
        HttpResponseMessage response = await http.PostAsJsonAsync("/Auth/login", new { email, password });

        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new InvalidOperationException(
                $"Giris basarisiz ({(int)response.StatusCode}): {await response.Content.ReadAsStringAsync()}");
        }

        LoginResponse? body = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.Token);
        return this;
    }

    public void ClearToken() => http.DefaultRequestHeaders.Authorization = null;

    public Task<HttpResponseMessage> GetAsync(string url) => http.GetAsync(url);

    public Task<HttpResponseMessage> PostAsync(string url, object body) =>
        http.PostAsJsonAsync(url, body);

    public Task<HttpResponseMessage> PutAsync(string url, object? body = null) =>
        http.PutAsJsonAsync(url, body ?? new { });

    public Task<HttpResponseMessage> DeleteAsync(string url) => http.DeleteAsync(url);

    public async Task<T> GetJsonAsync<T>(string url)
    {
        HttpResponseMessage response = await http.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>(JsonOptions))!;
    }
}

public record LoginResponse(string Token, LoginUser User);

public record LoginUser(string Email, string Rol, int? PersonelId, bool IsFirstLogin);

public record PersonelResponse(
    int Id,
    string Ad,
    string Soyad,
    string Departman,
    string Unvan,
    decimal Maas,
    string IseBaslamaTarihi,
    string Email,
    string? Telefon,
    string? Adres,
    string? Iban,
    string? DogumTarihi,
    bool AktifMi,
    string? IseCikisTarihi,
    List<ExperienceResponse> Experiences);

public record ExperienceResponse(string Company, string Role, string Duration);

public record IzinTalepResponse(
    int Id,
    int PersonelId,
    string PersonelAdSoyad,
    int? SubstituteId,
    string? SubstituteAdSoyad,
    string BaslangicTarihi,
    string BitisTarihi,
    int GunSayisi,
    string Turu,
    string Durum,
    string TalepTarihi,
    string? Aciklama);

public record IzinOzetResponse(
    int Yil,
    int HakEdilenGun,
    int DevredenGun,
    int ToplamHak,
    int KullanilanGun,
    int KalanGun,
    int KullanilanMazeretGun,
    int KullanilanUcretsizGun,
    List<IzinTalepResponse> Gecmis);

public record IzinHakkiResponse(
    int Yil,
    int HakEdilenGun,
    int DevredenGun,
    int ToplamHak,
    int KullanilanGun,
    int KalanGun,
    int KullanilanMazeretGun,
    int KullanilanUcretsizGun);

public record SubstituteCandidateResponse(int Id, string AdSoyad, string Unvan);

public record ZimmetResponse(
    int Id,
    int PersonelId,
    string PersonelAdSoyad,
    string EsyaAdi,
    string? SeriNo,
    string ZimmetTarihi,
    string? IadeTarihi,
    string? Aciklama);
