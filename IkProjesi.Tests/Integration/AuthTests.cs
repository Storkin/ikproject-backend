using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IkProjesi.Tests.Infrastructure;
using Xunit;

namespace IkProjesi.Tests.Integration;

public class AuthTests : IntegrationTestBase
{
    public AuthTests(ApiFixture fixture) : base(fixture) { }

    [Theory]
    [InlineData(TestUsers.IkEmail, TestUsers.IkPassword, "IkYonetici")]
    [InlineData(TestUsers.AdminEmail, TestUsers.AdminPassword, "Admin")]
    [InlineData(TestUsers.CalisanEmail, TestUsers.CalisanPassword, "Calisan")]
    public async Task Login_DogruBilgilerle_TokenVeRolDoner(string email, string sifre, string beklenenRol)
    {
        ApiClient client = Fixture.NewClient();

        HttpResponseMessage response = await client.PostAsync("/Auth/login", new { email, password = sifre });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        LoginResponse body = (await response.Content.ReadFromJsonAsync<LoginResponse>(ApiClient.JsonOptions))!;
        body.Token.Should().NotBeNullOrWhiteSpace();
        body.User.Rol.Should().Be(beklenenRol);
    }

    [Fact]
    public async Task Login_YanlisSifre_401Doner_VeSifreHashiSizmaz()
    {
        ApiClient client = Fixture.NewClient();

        HttpResponseMessage response = await client.PostAsync("/Auth/login",
            new { email = TestUsers.IkEmail, password = "AcikcaYanlis1!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        string govde = await response.Content.ReadAsStringAsync();
        govde.Should().NotContain("$2a$", "BCrypt hash'i hicbir yanitta gorunmemeli");
    }

    [Fact]
    public async Task Login_OlmayanKullanici_401Doner()
    {
        ApiClient client = Fixture.NewClient();

        HttpResponseMessage response = await client.PostAsync("/Auth/login",
            new { email = "hicyok@test.com", password = "Deneme123!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_PasifePersonel_GirisEngellenir_VeAyriMesajDoner()
    {
        ApiClient ik = await Fixture.IkClientAsync();
        PersonelResponse personel = await ik.GetJsonAsync<PersonelResponse>("/Personnel/getById/1");

        await ik.DeleteAsync($"/Personnel/deletePersonnel/{personel.Id}");

        ApiClient client = Fixture.NewClient();
        HttpResponseMessage response = await client.PostAsync("/Auth/login",
            new { email = TestUsers.CalisanEmail, password = TestUsers.CalisanPassword });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        string mesaj = await response.Content.ReadAsStringAsync();
        mesaj.Should().Contain("aktif değil",
            "kullanici sifresini yanlis girdigini sanmasin diye ayirt edici mesaj gerekir");
    }

    [Fact]
    public async Task ChangePassword_MevcutSifreYanlis_400Doner_VeSifreDegismez()
    {
        ApiClient calisan = await Fixture.CalisanClientAsync();

        HttpResponseMessage response = await calisan.PutAsync("/Auth/changePassword",
            new { currentPassword = "TamamenYanlis1!", newPassword = "YeniSifre1!" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Eski sifre hala gecerli olmali
        ApiClient tekrar = Fixture.NewClient();
        HttpResponseMessage login = await tekrar.PostAsync("/Auth/login",
            new { email = TestUsers.CalisanEmail, password = TestUsers.CalisanPassword });
        login.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangePassword_MevcutSifreDogru_SifreDegisir_VeEskisiGecersizlesir()
    {
        ApiClient calisan = await Fixture.CalisanClientAsync();
        const string yeniSifre = "BambaskaSifre1!";

        HttpResponseMessage response = await calisan.PutAsync("/Auth/changePassword",
            new { currentPassword = TestUsers.CalisanPassword, newPassword = yeniSifre });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        ApiClient client = Fixture.NewClient();
        (await client.PostAsync("/Auth/login",
            new { email = TestUsers.CalisanEmail, password = TestUsers.CalisanPassword }))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized, "eski sifre artik calismamali");

        (await client.PostAsync("/Auth/login",
            new { email = TestUsers.CalisanEmail, password = yeniSifre }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangePassword_IlkGirisBayraginiKapatir()
    {
        ApiClient ik = await Fixture.IkClientAsync();

        // Yeni personel -> otomatik hesap -> isFirstLogin true
        HttpResponseMessage ekle = await ik.PostAsync("/Personnel/addPersonnel", new
        {
            ad = "Deniz",
            soyad = "Yildiz",
            email = "deniz@test.com",
            departman = "Muhasebe",
            unvan = "Uzman",
            maas = 30000,
            iseBaslamaTarihi = "2026-01-01",
            experiences = Array.Empty<object>()
        });
        ekle.StatusCode.Should().Be(HttpStatusCode.Created);

        ApiClient yeni = Fixture.NewClient();
        LoginResponse ilk = (await (await yeni.PostAsync("/Auth/login",
            new { email = "deniz@test.com", password = "Deniz123!" }))
            .Content.ReadFromJsonAsync<LoginResponse>(ApiClient.JsonOptions))!;

        ilk.User.IsFirstLogin.Should().BeTrue("yeni acilan hesap sifre degistirmeye yonlendirilmeli");

        await yeni.LoginAsAsync("deniz@test.com", "Deniz123!");
        await yeni.PutAsync("/Auth/changePassword",
            new { currentPassword = "Deniz123!", newPassword = "DenizYeni1!" });

        LoginResponse sonra = (await (await Fixture.NewClient().PostAsync("/Auth/login",
            new { email = "deniz@test.com", password = "DenizYeni1!" }))
            .Content.ReadFromJsonAsync<LoginResponse>(ApiClient.JsonOptions))!;

        sonra.User.IsFirstLogin.Should().BeFalse();
    }

    [Fact]
    public async Task ResetPassword_IkTarafindan_VarsayilanaDoner_VeIlkGirisTekrarAcilir()
    {
        ApiClient ik = await Fixture.IkClientAsync();

        HttpResponseMessage response = await ik.PutAsync("/Auth/resetPassword",
            new { email = TestUsers.CalisanEmail });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Ahmet123!");

        LoginResponse login = (await (await Fixture.NewClient().PostAsync("/Auth/login",
            new { email = TestUsers.CalisanEmail, password = "Ahmet123!" }))
            .Content.ReadFromJsonAsync<LoginResponse>(ApiClient.JsonOptions))!;

        login.User.IsFirstLogin.Should().BeTrue();
    }

    [Fact]
    public async Task ResetPassword_CalisanCagiramaz()
    {
        ApiClient calisan = await Fixture.CalisanClientAsync();

        HttpResponseMessage response = await calisan.PutAsync("/Auth/resetPassword",
            new { email = TestUsers.IkEmail });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Register_TokensizCagrilamaz_YetkiYukseltmeEngellenir()
    {
        ApiClient client = Fixture.NewClient();

        HttpResponseMessage response = await client.PostAsync("/Auth/register", new
        {
            email = "saldirgan@test.com",
            password = "Saldiri123!",
            rol = "IkYonetici"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "aksi halde disaridan herkes kendine IkYonetici hesabi acabilir");
    }

    [Fact]
    public async Task Register_CalisanTarafindanCagrilamaz()
    {
        ApiClient calisan = await Fixture.CalisanClientAsync();

        HttpResponseMessage response = await calisan.PostAsync("/Auth/register", new
        {
            email = "yukseltme@test.com",
            password = "Yukselt123!",
            rol = "IkYonetici"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task KorumaliUc_TokensizCagrilirsa_401Doner()
    {
        ApiClient client = Fixture.NewClient();

        HttpResponseMessage response = await client.GetAsync("/Personnel/getPersonnel");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task KorumaliUc_BozukTokenla_401Doner()
    {
        ApiClient client = Fixture.NewClient();
        client.Raw.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "tamamen.uydurma.token");

        HttpResponseMessage response = await client.GetAsync("/Personnel/getPersonnel");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
