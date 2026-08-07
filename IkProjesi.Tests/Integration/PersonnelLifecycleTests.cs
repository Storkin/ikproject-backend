using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IkProjesi.Tests.Infrastructure;
using Xunit;

namespace IkProjesi.Tests.Integration;

/// <summary>
/// Personel yasam dongusu: ekleme -> otomatik hesap -> guncelleme ->
/// isten cikarma (soft delete) -> geri ise alma.
/// </summary>
public class PersonnelLifecycleTests : IntegrationTestBase
{
    public PersonnelLifecycleTests(ApiFixture fixture) : base(fixture) { }

    private static object YeniPersonel(
        string ad = "Deniz",
        string soyad = "Yildiz",
        string email = "deniz@test.com",
        string departman = "Muhasebe",
        string unvan = "Uzman",
        object[]? experiences = null) =>
        new
        {
            ad,
            soyad,
            email,
            departman,
            unvan,
            maas = 35000,
            iseBaslamaTarihi = "2026-01-15",
            telefon = "5551112233",
            adres = "Ankara",
            iban = "TR000000000000000000000000",
            dogumTarihi = "1998-04-20",
            experiences = experiences ?? Array.Empty<object>()
        };

    [Fact]
    public async Task PersonelEkleme_TumAlanlariKaydeder()
    {
        ApiClient ik = await Fixture.IkClientAsync();

        HttpResponseMessage response = await ik.PostAsync("/Personnel/addPersonnel", YeniPersonel());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        PersonelResponse olusan = (await response.Content.ReadFromJsonAsync<PersonelResponse>(ApiClient.JsonOptions))!;

        olusan.Ad.Should().Be("Deniz");
        olusan.Departman.Should().Be("Muhasebe");
        olusan.Unvan.Should().Be("Uzman");
        olusan.Telefon.Should().Be("5551112233");
        olusan.AktifMi.Should().BeTrue();
        olusan.IseCikisTarihi.Should().BeNull();
    }

    [Fact]
    public async Task PersonelEkleme_UretilenGeciciSifreyiYanittaDoner()
    {
        ApiClient ik = await Fixture.IkClientAsync();

        HttpResponseMessage response = await ik.PostAsync("/Personnel/addPersonnel",
            YeniPersonel(ad: "mustafa", email: "mustafa@test.com"));

        PersonelResponse olusan = (await response.Content.ReadFromJsonAsync<PersonelResponse>(ApiClient.JsonOptions))!;
        olusan.GeciciSifre.Should().Be("Mustafa123!",
            "IK acilan hesabin sifresini tahmin etmek zorunda kalmamali");

        (await Fixture.NewClient().PostAsync("/Auth/login",
            new { email = "mustafa@test.com", password = olusan.GeciciSifre }))
            .StatusCode.Should().Be(HttpStatusCode.OK, "donen sifre gercekten calismali");
    }

    [Fact]
    public async Task PersonelListeleme_GeciciSifreAlaniniIcermez()
    {
        ApiClient ik = await Fixture.IkClientAsync();
        await ik.PostAsync("/Personnel/addPersonnel", YeniPersonel());

        List<PersonelResponse> liste = await ik.GetJsonAsync<List<PersonelResponse>>("/Personnel/getPersonnel");

        liste.Should().OnlyContain(p => p.GeciciSifre == null,
            "gecici sifre yalnizca olusturma aninda gorunmeli");
    }

    [Fact]
    public async Task PersonelEkleme_OtomatikGirisHesabiAcar()
    {
        ApiClient ik = await Fixture.IkClientAsync();
        await ik.PostAsync("/Personnel/addPersonnel", YeniPersonel(ad: "Mehmet", email: "mehmet@test.com"));

        // Sifre kurali: Ad + "123!"
        ApiClient yeni = Fixture.NewClient();
        HttpResponseMessage login = await yeni.PostAsync("/Auth/login",
            new { email = "mehmet@test.com", password = "Mehmet123!" });

        login.StatusCode.Should().Be(HttpStatusCode.OK);
        LoginResponse body = (await login.Content.ReadFromJsonAsync<LoginResponse>(ApiClient.JsonOptions))!;
        body.User.Rol.Should().Be("Calisan");
        body.User.PersonelId.Should().NotBeNull("hesap dogru personele baglanmali");
    }

    [Theory]
    [InlineData("MEHMET", "Mehmet123!")]
    [InlineData("mehmet", "Mehmet123!")]
    [InlineData("  Mehmet  ", "Mehmet123!")]
    public async Task VarsayilanSifre_AdYazimindanBagimsizUretilir(string ad, string beklenenSifre)
    {
        ApiClient ik = await Fixture.IkClientAsync();
        await ik.PostAsync("/Personnel/addPersonnel", YeniPersonel(ad: ad, email: "yazim@test.com"));

        HttpResponseMessage login = await Fixture.NewClient().PostAsync("/Auth/login",
            new { email = "yazim@test.com", password = beklenenSifre });

        login.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task IsDeneyimi_PersonelKaydiylaBirlikteGidipGelir()
    {
        ApiClient ik = await Fixture.IkClientAsync();
        object[] deneyimler =
        {
            new { company = "ABC Yazilim", role = "Junior Developer", duration = "2 yil" },
            new { company = "XYZ Teknoloji", role = "Developer", duration = "3 yil" }
        };

        HttpResponseMessage response = await ik.PostAsync("/Personnel/addPersonnel",
            YeniPersonel(experiences: deneyimler));

        PersonelResponse olusan = (await response.Content.ReadFromJsonAsync<PersonelResponse>(ApiClient.JsonOptions))!;
        olusan.Experiences.Should().HaveCount(2);
        olusan.Experiences[0].Company.Should().Be("ABC Yazilim");

        PersonelResponse okunan = await ik.GetJsonAsync<PersonelResponse>($"/Personnel/getById/{olusan.Id}");
        okunan.Experiences.Should().HaveCount(2, "kayit kalici olmali");
    }

    [Fact]
    public async Task IsDeneyimi_GuncellemedeListeYenisiyleDegistirilir()
    {
        ApiClient ik = await Fixture.IkClientAsync();
        HttpResponseMessage ekle = await ik.PostAsync("/Personnel/addPersonnel", YeniPersonel(experiences: new object[]
        {
            new { company = "Eski Sirket", role = "Stajyer", duration = "1 yil" }
        }));
        PersonelResponse olusan = (await ekle.Content.ReadFromJsonAsync<PersonelResponse>(ApiClient.JsonOptions))!;

        await ik.PutAsync($"/Personnel/updatePersonnel/{olusan.Id}", new
        {
            ad = olusan.Ad,
            soyad = olusan.Soyad,
            email = olusan.Email,
            departman = "Muhasebe",
            unvan = "Uzman",
            maas = 40000,
            iseBaslamaTarihi = "2026-01-15",
            experiences = new object[]
            {
                new { company = "Yeni Sirket", role = "Uzman", duration = "2 yil" }
            }
        });

        PersonelResponse guncel = await ik.GetJsonAsync<PersonelResponse>($"/Personnel/getById/{olusan.Id}");
        guncel.Experiences.Should().ContainSingle();
        guncel.Experiences[0].Company.Should().Be("Yeni Sirket", "eski liste yerine yenisi yazilmali");
    }

    [Fact]
    public async Task BosDeneyimSatirlari_Atlanir()
    {
        ApiClient ik = await Fixture.IkClientAsync();

        HttpResponseMessage response = await ik.PostAsync("/Personnel/addPersonnel", YeniPersonel(experiences: new object[]
        {
            new { company = "Gercek Sirket", role = "Developer", duration = "2 yil" },
            new { company = "", role = "", duration = "" }
        }));

        PersonelResponse olusan = (await response.Content.ReadFromJsonAsync<PersonelResponse>(ApiClient.JsonOptions))!;
        olusan.Experiences.Should().ContainSingle("bos satir kaydedilmemeli");
    }

    // ---------- ISTEN CIKARMA (SOFT DELETE) ----------

    [Fact]
    public async Task IstenCikarma_KaydiSilmez_PasifeAlir()
    {
        ApiClient ik = await Fixture.IkClientAsync();
        List<PersonelResponse> hepsi = await ik.GetJsonAsync<List<PersonelResponse>>("/Personnel/getPersonnel");
        int id = hepsi.First().Id;

        (await ik.DeleteAsync($"/Personnel/deletePersonnel/{id}")).StatusCode
            .Should().Be(HttpStatusCode.NoContent);

        List<PersonelResponse> aktifler = await ik.GetJsonAsync<List<PersonelResponse>>("/Personnel/getPersonnel");
        aktifler.Should().NotContain(p => p.Id == id, "varsayilan listede gorunmemeli");

        List<PersonelResponse> tumu = await ik.GetJsonAsync<List<PersonelResponse>>(
            "/Personnel/getPersonnel?includeInactive=true");
        PersonelResponse ayrilan = tumu.Single(p => p.Id == id);
        ayrilan.AktifMi.Should().BeFalse();
        ayrilan.IseCikisTarihi.Should().NotBeNull();
    }

    [Fact]
    public async Task IstenCikarma_IzinGecmisiniKorur()
    {
        ApiClient calisan = await Fixture.CalisanClientAsync();
        ApiClient ik = await Fixture.IkClientAsync();

        await calisan.PostAsync("/Leave/createLeave", new
        {
            baslangicTarihi = "2026-09-01",
            bitisTarihi = "2026-09-03",
            turu = "Yillik"
        });

        List<PersonelResponse> hepsi = await ik.GetJsonAsync<List<PersonelResponse>>("/Personnel/getPersonnel");
        int id = hepsi.Single(p => p.Email == TestUsers.CalisanEmail).Id;

        await ik.DeleteAsync($"/Personnel/deletePersonnel/{id}");

        IzinOzetResponse gecmis = await ik.GetJsonAsync<IzinOzetResponse>($"/Leave/getPersonnelHistory/{id}");
        gecmis.Gecmis.Should().NotBeEmpty("isten ayrilanin izin gecmisi saklanmali");
    }

    [Fact]
    public async Task IkinciKezIstenCikarma_Basarisiz()
    {
        ApiClient ik = await Fixture.IkClientAsync();
        List<PersonelResponse> hepsi = await ik.GetJsonAsync<List<PersonelResponse>>("/Personnel/getPersonnel");
        int id = hepsi.First().Id;

        await ik.DeleteAsync($"/Personnel/deletePersonnel/{id}");

        (await ik.DeleteAsync($"/Personnel/deletePersonnel/{id}")).StatusCode
            .Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GeriIseAlma_PersoneliAktifYapar_VeGirisiAcar()
    {
        ApiClient ik = await Fixture.IkClientAsync();
        List<PersonelResponse> hepsi = await ik.GetJsonAsync<List<PersonelResponse>>("/Personnel/getPersonnel");
        int id = hepsi.Single(p => p.Email == TestUsers.CalisanEmail).Id;

        await ik.DeleteAsync($"/Personnel/deletePersonnel/{id}");
        (await Fixture.NewClient().PostAsync("/Auth/login",
            new { email = TestUsers.CalisanEmail, password = TestUsers.CalisanPassword }))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        (await ik.PutAsync($"/Personnel/reactivatePersonnel/{id}")).StatusCode.Should().Be(HttpStatusCode.OK);

        (await Fixture.NewClient().PostAsync("/Auth/login",
            new { email = TestUsers.CalisanEmail, password = TestUsers.CalisanPassword }))
            .StatusCode.Should().Be(HttpStatusCode.OK, "geri alinan personel tekrar girebilmeli");
    }

    [Fact]
    public async Task ZatenAktifPersonel_TekrarIseAlinamaz()
    {
        ApiClient ik = await Fixture.IkClientAsync();
        List<PersonelResponse> hepsi = await ik.GetJsonAsync<List<PersonelResponse>>("/Personnel/getPersonnel");

        (await ik.PutAsync($"/Personnel/reactivatePersonnel/{hepsi.First().Id}")).StatusCode
            .Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AyniEmailIleYenidenIseAlim_HesabiYeniKaydaBaglar()
    {
        ApiClient ik = await Fixture.IkClientAsync();
        List<PersonelResponse> hepsi = await ik.GetJsonAsync<List<PersonelResponse>>("/Personnel/getPersonnel");
        int eskiId = hepsi.Single(p => p.Email == TestUsers.CalisanEmail).Id;

        await ik.DeleteAsync($"/Personnel/deletePersonnel/{eskiId}");

        HttpResponseMessage yeniden = await ik.PostAsync("/Personnel/addPersonnel",
            YeniPersonel(ad: "Ahmet", soyad: "Yilmaz", email: TestUsers.CalisanEmail));
        yeniden.StatusCode.Should().Be(HttpStatusCode.Created);
        PersonelResponse yeniKayit = (await yeniden.Content.ReadFromJsonAsync<PersonelResponse>(ApiClient.JsonOptions))!;

        HttpResponseMessage login = await Fixture.NewClient().PostAsync("/Auth/login",
            new { email = TestUsers.CalisanEmail, password = "Ahmet123!" });

        login.StatusCode.Should().Be(HttpStatusCode.OK, "eski hesap yeni kayda baglanmali");
        LoginResponse body = (await login.Content.ReadFromJsonAsync<LoginResponse>(ApiClient.JsonOptions))!;
        body.User.PersonelId.Should().Be(yeniKayit.Id, "hesap eski pasif kayda degil yenisine bakmali");
    }

    // ---------- CALISAN PROFILI ----------

    [Fact]
    public async Task Calisan_KendiIletisimBilgileriniGunceller_HassasAlanlarDegismez()
    {
        ApiClient calisan = await Fixture.CalisanClientAsync();
        PersonelResponse once = await calisan.GetJsonAsync<PersonelResponse>("/Personnel/getProfile");

        (await calisan.PutAsync("/Personnel/updateProfile", new
        {
            email = once.Email,
            telefon = "5559998877",
            adres = "Yeni Adres",
            iban = "TR111111111111111111111111"
        })).StatusCode.Should().Be(HttpStatusCode.NoContent);

        PersonelResponse sonra = await calisan.GetJsonAsync<PersonelResponse>("/Personnel/getProfile");
        sonra.Telefon.Should().Be("5559998877");
        sonra.Maas.Should().Be(once.Maas, "calisan kendi maasini degistirememeli");
        sonra.Unvan.Should().Be(once.Unvan, "calisan kendi unvanini degistirememeli");
        sonra.Departman.Should().Be(once.Departman);
    }

    [Fact]
    public async Task Calisan_PersonelYonetimUclarinaErisemez()
    {
        ApiClient calisan = await Fixture.CalisanClientAsync();

        (await calisan.GetAsync("/Personnel/getPersonnel")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await calisan.GetAsync("/Personnel/getById/1")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await calisan.GetAsync("/Personnel/getBySalary")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await calisan.DeleteAsync("/Personnel/deletePersonnel/1")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Admin_IkIleAyniYetkilereSahip()
    {
        ApiClient admin = await Fixture.AdminClientAsync();

        (await admin.GetAsync("/Personnel/getPersonnel")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await admin.GetAsync("/Leave/getPending")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await admin.GetAsync("/Equipment/getAllEquipment")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ---------- ARAMA / FILTRELEME ----------

    [Fact]
    public async Task DepartmanFiltresi_SadeceODepartmaniDoner()
    {
        ApiClient ik = await Fixture.IkClientAsync();

        List<PersonelResponse> muhasebe =
            await ik.GetJsonAsync<List<PersonelResponse>>("/Personnel/getByDepartment/Muhasebe");

        muhasebe.Should().OnlyContain(p => p.Departman == "Muhasebe");
        muhasebe.Should().HaveCount(2);
    }

    [Fact]
    public async Task Arama_BosKelimeyle_400Doner()
    {
        ApiClient ik = await Fixture.IkClientAsync();

        (await ik.GetAsync("/Personnel/search?keyword=")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Arama_BuyukKucukHarfDuyarsiz()
    {
        ApiClient ik = await Fixture.IkClientAsync();

        List<PersonelResponse> sonuc = await ik.GetJsonAsync<List<PersonelResponse>>("/Personnel/search?keyword=AHMET");

        sonuc.Should().ContainSingle(p => p.Ad == "Ahmet");
    }

    [Fact]
    public async Task OlmayanPersonel_404Doner()
    {
        ApiClient ik = await Fixture.IkClientAsync();

        (await ik.GetAsync("/Personnel/getById/999999")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
