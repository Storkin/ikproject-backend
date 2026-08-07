using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using IkProjesi.Tests.Infrastructure;
using Xunit;

namespace IkProjesi.Tests.Integration;

/// <summary>
/// API sozlesmesi: JSON bicimleri, enum/tarih toleransi ve hatali girdiler.
/// Frontend bu bicimlere gore yazildigi icin bunlar kirilirsa arayuz coker.
/// </summary>
public class ApiContractTests : IntegrationTestBase
{
    public ApiContractTests(ApiFixture fixture) : base(fixture) { }

    private static object PersonelGovdesi(object? unvan = null, object? dogumTarihi = null, object? maas = null) =>
        new
        {
            ad = "Sozlesme",
            soyad = "Testi",
            email = "sozlesme@test.com",
            departman = "Muhasebe",
            unvan = unvan ?? "Uzman",
            maas = maas ?? 30000,
            iseBaslamaTarihi = "2026-01-15",
            dogumTarihi,
            experiences = Array.Empty<object>()
        };

    // ---------- TARIH BICIMI ----------

    [Fact]
    public async Task TarihAlanlari_SaatsizDoner()
    {
        ApiClient ik = await Fixture.IkClientAsync();

        string govde = await (await ik.GetAsync("/Personnel/getPersonnel")).Content.ReadAsStringAsync();

        govde.Should().Contain("\"iseBaslamaTarihi\":\"2024-");
        govde.Should().NotContain("T00:00:00", "tarihler saatsiz gonderilmeli");
        govde.Should().NotContain("Z\"", "UTC eki gorunmemeli");
    }

    [Fact]
    public async Task IzinTarihleri_SaatsizDoner()
    {
        ApiClient calisan = await Fixture.CalisanClientAsync();
        await calisan.PostAsync("/Leave/createLeave", new
        {
            baslangicTarihi = "2026-09-01",
            bitisTarihi = "2026-09-03",
            turu = "Yillik"
        });

        string govde = await (await calisan.GetAsync("/Leave/getMyLeaves")).Content.ReadAsStringAsync();

        govde.Should().Contain("\"baslangicTarihi\":\"2026-09-01\"");
        govde.Should().NotContain("T00:00:00");
    }

    // ---------- ENUM BICIMI ----------

    [Fact]
    public async Task Enumlar_SayiDegilMetinDoner()
    {
        ApiClient ik = await Fixture.IkClientAsync();

        string govde = await (await ik.GetAsync("/Personnel/getPersonnel")).Content.ReadAsStringAsync();

        govde.Should().Contain("\"departman\":\"Muhasebe\"");
        govde.Should().NotContain("\"departman\":0");
    }

    [Theory]
    [InlineData("Muhasebe")]
    [InlineData("muhasebe")]
    [InlineData("MUHASEBE")]
    public async Task EnumGirdisi_BuyukKucukHarfDuyarsiz(string departman)
    {
        ApiClient ik = await Fixture.IkClientAsync();

        HttpResponseMessage response = await ik.PostAsync("/Personnel/addPersonnel", new
        {
            ad = "Harf",
            soyad = "Testi",
            email = $"harf{departman}@test.com",
            departman,
            unvan = "Uzman",
            maas = 30000,
            iseBaslamaTarihi = "2026-01-15",
            experiences = Array.Empty<object>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GecersizEnumDegeri_400Doner()
    {
        ApiClient ik = await Fixture.IkClientAsync();

        HttpResponseMessage response = await ik.PostAsync("/Personnel/addPersonnel", new
        {
            ad = "Gecersiz",
            soyad = "Departman",
            email = "gecersiz@test.com",
            departman = "UyduruDepartman",
            unvan = "Uzman",
            maas = 30000,
            iseBaslamaTarihi = "2026-01-15",
            experiences = Array.Empty<object>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "tanimsiz enum degeri sessizce varsayilana dusmemeli");
    }

    // ---------- BOS ALAN TOLERANSI ----------
    // Frontend doldurulmamis alanlari "" olarak gonderiyor; bunlar 400 uretmemeli.

    [Fact]
    public async Task BosEnumAlani_VarsayilanaDuser()
    {
        ApiClient ik = await Fixture.IkClientAsync();

        HttpResponseMessage response = await ik.PostAsync("/Personnel/addPersonnel",
            PersonelGovdesi(unvan: ""));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        PersonelResponse olusan = (await response.Content.ReadFromJsonAsync<PersonelResponse>(ApiClient.JsonOptions))!;
        olusan.Unvan.Should().Be("Stajyer", "enum'un ilk degeri varsayilan olmali");
    }

    [Fact]
    public async Task BosTarihAlani_NullOlarakKaydedilir()
    {
        ApiClient ik = await Fixture.IkClientAsync();

        HttpResponseMessage response = await ik.PostAsync("/Personnel/addPersonnel",
            PersonelGovdesi(dogumTarihi: ""));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        PersonelResponse olusan = (await response.Content.ReadFromJsonAsync<PersonelResponse>(ApiClient.JsonOptions))!;
        olusan.DogumTarihi.Should().BeNull();
    }

    [Fact]
    public async Task SayiAlani_MetinOlarakGonderilebilir()
    {
        ApiClient ik = await Fixture.IkClientAsync();

        HttpResponseMessage response = await ik.PostAsync("/Personnel/addPersonnel",
            PersonelGovdesi(maas: "42000"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        PersonelResponse olusan = (await response.Content.ReadFromJsonAsync<PersonelResponse>(ApiClient.JsonOptions))!;
        olusan.Maas.Should().Be(42000);
    }

    [Fact]
    public async Task IzinTuru_GonderilmezseYillikVarsayilir()
    {
        ApiClient calisan = await Fixture.CalisanClientAsync();

        HttpResponseMessage response = await calisan.PostAsync("/Leave/createLeave", new
        {
            baslangicTarihi = "2026-09-01",
            bitisTarihi = "2026-09-03"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<IzinTalepResponse> talepler = await calisan.GetJsonAsync<List<IzinTalepResponse>>("/Leave/getMyLeaves");
        talepler.Single().Turu.Should().Be("Yillik", "eski istemciler kirilmamali");
    }

    // ---------- BOZUK GIRDI ----------

    [Fact]
    public async Task BozukJson_400Doner_500Degil()
    {
        ApiClient ik = await Fixture.IkClientAsync();
        StringContent bozuk = new("{ bu gecerli json degil", Encoding.UTF8, "application/json");

        HttpResponseMessage response = await ik.Raw.PostAsync("/Personnel/addPersonnel", bozuk);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "bozuk govde sunucu hatasina donusmemeli");
    }

    [Fact]
    public async Task GecersizTarihMetni_500Uretmez()
    {
        ApiClient ik = await Fixture.IkClientAsync();

        HttpResponseMessage response = await ik.PostAsync("/Personnel/addPersonnel", new
        {
            ad = "Bozuk",
            soyad = "Tarih",
            email = "bozuktarih@test.com",
            departman = "Muhasebe",
            unvan = "Uzman",
            maas = 30000,
            iseBaslamaTarihi = "asdasd",
            experiences = Array.Empty<object>()
        });

        ((int)response.StatusCode).Should().BeLessThan(500,
            "gecersiz tarih kullanici hatasidir, sunucu hatasi degil");
    }

    [Fact]
    public async Task SifreHashi_HicbirYanittaDonmez()
    {
        ApiClient ik = await Fixture.IkClientAsync();

        string personelGovde = await (await ik.GetAsync("/Personnel/getPersonnel")).Content.ReadAsStringAsync();
        string loginGovde = await (await Fixture.NewClient().PostAsync("/Auth/login",
            new { email = TestUsers.IkEmail, password = TestUsers.IkPassword })).Content.ReadAsStringAsync();

        personelGovde.Should().NotContainEquivalentOf("passwordHash");
        loginGovde.Should().NotContainEquivalentOf("passwordHash");
        loginGovde.Should().NotContain("$2a$");
    }
}
