using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IkProjesi.Tests.Infrastructure;
using Xunit;

namespace IkProjesi.Tests.Integration;

/// <summary>
/// Zimmet, egitim ve maas modulleri: CRUD, dogrulamalar ve veri izolasyonu.
/// </summary>
public class AssetAndRecordTests : IntegrationTestBase
{
    public AssetAndRecordTests(ApiFixture fixture) : base(fixture) { }

    private async Task<int> CalisanPersonelIdAsync(ApiClient ik)
    {
        List<PersonelResponse> hepsi = await ik.GetJsonAsync<List<PersonelResponse>>("/Personnel/getPersonnel");
        return hepsi.Single(p => p.Email == TestUsers.CalisanEmail).Id;
    }

    // ---------- ZIMMET ----------

    [Fact]
    public async Task Zimmet_AtamaVeIade_TamAkis()
    {
        ApiClient ik = await Fixture.IkClientAsync();
        int personelId = await CalisanPersonelIdAsync(ik);

        (await ik.PostAsync("/Equipment/assign", new
        {
            personelId,
            esyaAdi = "Dell Latitude",
            seriNo = "DL-001",
            zimmetTarihi = "2026-03-01",
            aciklama = "is laptopu"
        })).StatusCode.Should().Be(HttpStatusCode.OK);

        List<ZimmetResponse> liste = await ik.GetJsonAsync<List<ZimmetResponse>>($"/Equipment/getByPersonnel/{personelId}");
        liste.Should().ContainSingle();
        liste[0].IadeTarihi.Should().BeNull("yeni zimmet iade edilmemis olmali");
        liste[0].PersonelAdSoyad.Should().Be("Ahmet Yilmaz");

        (await ik.PutAsync($"/Equipment/return/{liste[0].Id}")).StatusCode.Should().Be(HttpStatusCode.OK);

        List<ZimmetResponse> sonra = await ik.GetJsonAsync<List<ZimmetResponse>>($"/Equipment/getByPersonnel/{personelId}");
        sonra.Should().ContainSingle("iade kaydi silmemeli, gecmis korunmali");
        sonra[0].IadeTarihi.Should().NotBeNull();
    }

    [Fact]
    public async Task Zimmet_IkinciKezIadeEdilemez()
    {
        ApiClient ik = await Fixture.IkClientAsync();
        int personelId = await CalisanPersonelIdAsync(ik);

        await ik.PostAsync("/Equipment/assign", new
        {
            personelId,
            esyaAdi = "Monitor",
            zimmetTarihi = "2026-03-01"
        });
        List<ZimmetResponse> liste = await ik.GetJsonAsync<List<ZimmetResponse>>($"/Equipment/getByPersonnel/{personelId}");
        await ik.PutAsync($"/Equipment/return/{liste[0].Id}");

        HttpResponseMessage ikinci = await ik.PutAsync($"/Equipment/return/{liste[0].Id}");

        ikinci.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ikinci.Content.ReadAsStringAsync()).Should().Contain("zaten iade");
    }

    [Fact]
    public async Task Zimmet_BosEsyaAdi_Reddedilir()
    {
        ApiClient ik = await Fixture.IkClientAsync();
        int personelId = await CalisanPersonelIdAsync(ik);

        HttpResponseMessage response = await ik.PostAsync("/Equipment/assign", new
        {
            personelId,
            esyaAdi = "   ",
            zimmetTarihi = "2026-03-01"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Zimmet_OlmayanPersonele_Atanamaz()
    {
        ApiClient ik = await Fixture.IkClientAsync();

        HttpResponseMessage response = await ik.PostAsync("/Equipment/assign", new
        {
            personelId = 999999,
            esyaAdi = "Laptop",
            zimmetTarihi = "2026-03-01"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Zimmet_CalisanSadeceKendininkiniGorur()
    {
        ApiClient ik = await Fixture.IkClientAsync();
        int ahmetId = await CalisanPersonelIdAsync(ik);
        List<PersonelResponse> hepsi = await ik.GetJsonAsync<List<PersonelResponse>>("/Personnel/getPersonnel");
        int zeynepId = hepsi.Single(p => p.Email == TestUsers.MeslektasEmail).Id;

        await ik.PostAsync("/Equipment/assign", new { personelId = ahmetId, esyaAdi = "Ahmet Laptop", zimmetTarihi = "2026-03-01" });
        await ik.PostAsync("/Equipment/assign", new { personelId = zeynepId, esyaAdi = "Zeynep Laptop", zimmetTarihi = "2026-03-01" });

        ApiClient ahmet = await Fixture.CalisanClientAsync();
        List<ZimmetResponse> benim = await ahmet.GetJsonAsync<List<ZimmetResponse>>("/Equipment/getMyEquipment");

        benim.Should().ContainSingle();
        benim[0].EsyaAdi.Should().Be("Ahmet Laptop");
    }

    [Fact]
    public async Task Zimmet_CalisanBaskaninZimmetineErisemez()
    {
        ApiClient calisan = await Fixture.CalisanClientAsync();

        (await calisan.GetAsync("/Equipment/getByPersonnel/2")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await calisan.GetAsync("/Equipment/getAllEquipment")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await calisan.PostAsync("/Equipment/assign", new { personelId = 1, esyaAdi = "X", zimmetTarihi = "2026-01-01" }))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ---------- EGITIM ----------

    [Fact]
    public async Task Egitim_EkleGuncelleSil_TamAkis()
    {
        ApiClient ik = await Fixture.IkClientAsync();
        int personelId = await CalisanPersonelIdAsync(ik);

        (await ik.PostAsync("/Education/add", new
        {
            personelId,
            ad = "AWS Cloud Practitioner",
            kurum = "Amazon",
            tamamlanmaTarihi = "2025-06-10",
            gecerlilikTarihi = "2028-06-10",
            aciklama = "bulut sertifikasi"
        })).StatusCode.Should().Be(HttpStatusCode.OK);

        List<JsonElementWrapper> liste = await ik.GetJsonAsync<List<JsonElementWrapper>>($"/Education/getByPersonnel/{personelId}");
        liste.Should().ContainSingle();
        int id = liste[0].Id;

        (await ik.PutAsync($"/Education/update/{id}", new
        {
            ad = "AWS Solutions Architect",
            kurum = "Amazon",
            tamamlanmaTarihi = "2025-06-10",
            gecerlilikTarihi = (string?)null,
            aciklama = "guncellendi"
        })).StatusCode.Should().Be(HttpStatusCode.OK);

        (await ik.DeleteAsync($"/Education/delete/{id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);

        List<JsonElementWrapper> sonra = await ik.GetJsonAsync<List<JsonElementWrapper>>($"/Education/getByPersonnel/{personelId}");
        sonra.Should().BeEmpty();
    }

    [Fact]
    public async Task Egitim_GecerlilikTarihiBosBirakilabilir()
    {
        ApiClient ik = await Fixture.IkClientAsync();
        int personelId = await CalisanPersonelIdAsync(ik);

        HttpResponseMessage response = await ik.PostAsync("/Education/add", new
        {
            personelId,
            ad = "Suresiz Sertifika",
            kurum = "Kurum",
            tamamlanmaTarihi = "2025-01-01",
            gecerlilikTarihi = (string?)null
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Egitim_BosAd_Reddedilir()
    {
        ApiClient ik = await Fixture.IkClientAsync();
        int personelId = await CalisanPersonelIdAsync(ik);

        HttpResponseMessage response = await ik.PostAsync("/Education/add", new
        {
            personelId,
            ad = "",
            kurum = "Kurum",
            tamamlanmaTarihi = "2025-01-01"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Egitim_CalisanSadeceGoruntuler_Ekleyemez()
    {
        ApiClient calisan = await Fixture.CalisanClientAsync();

        (await calisan.GetAsync("/Education/getMyEducation")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await calisan.PostAsync("/Education/add", new { personelId = 1, ad = "X", kurum = "Y", tamamlanmaTarihi = "2025-01-01" }))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ---------- MAAS ----------

    [Theory]
    [InlineData("Maas")]
    [InlineData("Prim")]
    [InlineData("Bonus")]
    public async Task Maas_TumTurlerKaydedilir(string tur)
    {
        ApiClient ik = await Fixture.IkClientAsync();
        int personelId = await CalisanPersonelIdAsync(ik);

        HttpResponseMessage response = await ik.PostAsync("/Salary/add", new
        {
            personelId,
            tutar = 5000,
            tur,
            gecerlilikTarihi = "2026-07-01",
            aciklama = "test"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-50000)]
    public async Task Maas_SifirVeyaNegatifTutar_Reddedilir(decimal tutar)
    {
        ApiClient ik = await Fixture.IkClientAsync();
        int personelId = await CalisanPersonelIdAsync(ik);

        HttpResponseMessage response = await ik.PostAsync("/Salary/add", new
        {
            personelId,
            tutar,
            tur = "Maas",
            gecerlilikTarihi = "2026-07-01"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Maas_CalisanBaskasininMaasiniGoremez()
    {
        ApiClient calisan = await Fixture.CalisanClientAsync();

        (await calisan.GetAsync("/Salary/getByPersonnel/2")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await calisan.GetAsync("/Salary/getMySalaryHistory")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Maas_CalisanKendiGecmisiniGorur()
    {
        ApiClient ik = await Fixture.IkClientAsync();
        int personelId = await CalisanPersonelIdAsync(ik);
        await ik.PostAsync("/Salary/add", new { personelId, tutar = 45000, tur = "Maas", gecerlilikTarihi = "2026-07-01" });

        ApiClient calisan = await Fixture.CalisanClientAsync();
        List<JsonElementWrapper> benim = await calisan.GetJsonAsync<List<JsonElementWrapper>>("/Salary/getMySalaryHistory");

        benim.Should().ContainSingle();
    }

    // ---------- CASCADE ----------

    [Fact]
    public async Task PersonelPasifeAlininca_ZimmetVeEgitimKayitlariKorunur()
    {
        ApiClient ik = await Fixture.IkClientAsync();
        int personelId = await CalisanPersonelIdAsync(ik);

        await ik.PostAsync("/Equipment/assign", new { personelId, esyaAdi = "Laptop", zimmetTarihi = "2026-03-01" });
        await ik.PostAsync("/Education/add", new { personelId, ad = "Sertifika", kurum = "Kurum", tamamlanmaTarihi = "2025-01-01" });
        await ik.PostAsync("/Salary/add", new { personelId, tutar = 45000, tur = "Maas", gecerlilikTarihi = "2026-07-01" });

        await ik.DeleteAsync($"/Personnel/deletePersonnel/{personelId}");

        (await ik.GetJsonAsync<List<ZimmetResponse>>($"/Equipment/getByPersonnel/{personelId}"))
            .Should().NotBeEmpty("isten ayrilanin zimmet gecmisi silinmemeli");
        (await ik.GetJsonAsync<List<JsonElementWrapper>>($"/Education/getByPersonnel/{personelId}"))
            .Should().NotBeEmpty();
        (await ik.GetJsonAsync<List<JsonElementWrapper>>($"/Salary/getByPersonnel/{personelId}"))
            .Should().NotBeEmpty();
    }
}

/// <summary>Sadece Id'ye ihtiyac duyulan listelerde kullanilan hafif model.</summary>
public record JsonElementWrapper(int Id);
