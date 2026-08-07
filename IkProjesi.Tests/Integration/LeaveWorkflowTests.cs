using System.Net;
using FluentAssertions;
using IkProjesi.Tests.Infrastructure;
using Xunit;

namespace IkProjesi.Tests.Integration;

/// <summary>
/// Izin talebi -> onay -> bakiye dusumu ucdan uca akisi ve tum sinir durumlari.
/// </summary>
public class LeaveWorkflowTests : IntegrationTestBase
{
    public LeaveWorkflowTests(ApiFixture fixture) : base(fixture) { }

    private static object Talep(string baslangic, string bitis, string turu = "Yillik",
        int? substituteId = null, string? aciklama = null) =>
        new
        {
            baslangicTarihi = baslangic,
            bitisTarihi = bitis,
            turu,
            substituteId,
            aciklama
        };

    private static async Task<int> BekleyenTalepIdAsync(ApiClient ik)
    {
        List<IzinTalepResponse> bekleyen = await ik.GetJsonAsync<List<IzinTalepResponse>>("/Leave/getPending");
        bekleyen.Should().NotBeEmpty("onaylanacak bir talep olmali");
        return bekleyen.Last().Id;
    }

    // ---------- MUTLU YOL ----------

    [Fact]
    public async Task TamAkis_TalepOlustur_Onayla_BakiyeDuser()
    {
        ApiClient calisan = await Fixture.CalisanClientAsync();
        ApiClient ik = await Fixture.IkClientAsync();

        IzinOzetResponse baslangic = await calisan.GetJsonAsync<IzinOzetResponse>("/Leave/getMySummary");
        baslangic.KalanGun.Should().Be(14);

        (await calisan.PostAsync("/Leave/createLeave", Talep("2026-09-01", "2026-09-03")))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        // Talep beklemede iken bakiye HENUZ dusmemeli
        IzinOzetResponse onayOncesi = await calisan.GetJsonAsync<IzinOzetResponse>("/Leave/getMySummary");
        onayOncesi.KullanilanGun.Should().Be(0, "hak ancak onaylandiginda dusmeli");

        int talepId = await BekleyenTalepIdAsync(ik);
        (await ik.PutAsync($"/Leave/approveLeave/{talepId}")).StatusCode.Should().Be(HttpStatusCode.OK);

        IzinOzetResponse sonrasi = await calisan.GetJsonAsync<IzinOzetResponse>("/Leave/getMySummary");
        sonrasi.KullanilanGun.Should().Be(3);
        sonrasi.KalanGun.Should().Be(11);
    }

    [Fact]
    public async Task Reddedilen_Talep_BakiyeyiEtkilemez()
    {
        ApiClient calisan = await Fixture.CalisanClientAsync();
        ApiClient ik = await Fixture.IkClientAsync();

        await calisan.PostAsync("/Leave/createLeave", Talep("2026-09-01", "2026-09-05"));
        int talepId = await BekleyenTalepIdAsync(ik);

        (await ik.PutAsync($"/Leave/rejectLeave/{talepId}")).StatusCode.Should().Be(HttpStatusCode.OK);

        IzinOzetResponse ozet = await calisan.GetJsonAsync<IzinOzetResponse>("/Leave/getMySummary");
        ozet.KullanilanGun.Should().Be(0);
        ozet.KalanGun.Should().Be(14);
    }

    [Fact]
    public async Task GunSayisi_IkiUcuDahilHesaplanir()
    {
        ApiClient calisan = await Fixture.CalisanClientAsync();

        await calisan.PostAsync("/Leave/createLeave", Talep("2026-09-01", "2026-09-01"));

        List<IzinTalepResponse> talepler = await calisan.GetJsonAsync<List<IzinTalepResponse>>("/Leave/getMyLeaves");
        talepler.Single().GunSayisi.Should().Be(1, "tek gunluk izin 1 gun sayilmali");
    }

    // ---------- IZIN TURU AYRIMI ----------

    [Theory]
    [InlineData("Mazeret")]
    [InlineData("Hastalik")]
    [InlineData("Ucretsiz")]
    public async Task YillikDisiTurler_YillikHaktanDusmez(string tur)
    {
        ApiClient calisan = await Fixture.CalisanClientAsync();
        ApiClient ik = await Fixture.IkClientAsync();

        await calisan.PostAsync("/Leave/createLeave", Talep("2026-09-01", "2026-09-04", tur));
        int talepId = await BekleyenTalepIdAsync(ik);
        await ik.PutAsync($"/Leave/approveLeave/{talepId}");

        IzinOzetResponse ozet = await calisan.GetJsonAsync<IzinOzetResponse>("/Leave/getMySummary");
        ozet.KullanilanGun.Should().Be(0, "yillik izin hakki korunmali");
        ozet.KalanGun.Should().Be(14);

        if (tur == "Ucretsiz")
        {
            ozet.KullanilanUcretsizGun.Should().Be(4);
        }
        else
        {
            ozet.KullanilanMazeretGun.Should().Be(4);
        }
    }

    [Fact]
    public async Task BakiyeBittiginde_YillikReddedilir_AmaMazeretKabulEdilir()
    {
        ApiClient calisan = await Fixture.CalisanClientAsync();
        ApiClient ik = await Fixture.IkClientAsync();

        // 14 gunun tamamini tuket
        await calisan.PostAsync("/Leave/createLeave", Talep("2026-03-01", "2026-03-14"));
        await ik.PutAsync($"/Leave/approveLeave/{await BekleyenTalepIdAsync(ik)}");

        HttpResponseMessage yillik = await calisan.PostAsync("/Leave/createLeave",
            Talep("2026-05-01", "2026-05-02"));
        yillik.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await yillik.Content.ReadAsStringAsync()).Should().Contain("Yetersiz izin hakkı");

        HttpResponseMessage mazeret = await calisan.PostAsync("/Leave/createLeave",
            Talep("2026-06-01", "2026-06-02", "Mazeret"));
        mazeret.StatusCode.Should().Be(HttpStatusCode.OK, "mazeret izni yillik bakiyeye bagli degil");
    }

    // ---------- TARIH DOGRULAMA ----------

    [Fact]
    public async Task BitisBaslangictanOnceyse_Reddedilir()
    {
        ApiClient calisan = await Fixture.CalisanClientAsync();

        HttpResponseMessage response = await calisan.PostAsync("/Leave/createLeave",
            Talep("2026-09-10", "2026-09-05"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ---------- CAKISMA KONTROLU ----------

    [Theory]
    [InlineData("2026-09-02", "2026-09-04", "icine giren")]
    [InlineData("2026-08-30", "2026-09-02", "basina binen")]
    [InlineData("2026-09-04", "2026-09-08", "sonuna binen")]
    [InlineData("2026-08-25", "2026-09-20", "tamamen kapsayan")]
    [InlineData("2026-09-01", "2026-09-05", "birebir ayni")]
    public async Task CakisanTarihAraliklari_Reddedilir(string bas, string bit, string senaryo)
    {
        ApiClient calisan = await Fixture.CalisanClientAsync();
        await calisan.PostAsync("/Leave/createLeave", Talep("2026-09-01", "2026-09-05", "Mazeret"));

        HttpResponseMessage response = await calisan.PostAsync("/Leave/createLeave",
            Talep(bas, bit, "Mazeret"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, $"senaryo: {senaryo}");
        (await response.Content.ReadAsStringAsync()).Should().Contain("zaten bir izin talebiniz var");
    }

    [Theory]
    [InlineData("2026-09-06", "2026-09-08", "hemen ertesi gun")]
    [InlineData("2026-08-28", "2026-08-31", "hemen oncesi")]
    public async Task BitisikAmaCakismayanTarihler_KabulEdilir(string bas, string bit, string senaryo)
    {
        ApiClient calisan = await Fixture.CalisanClientAsync();
        await calisan.PostAsync("/Leave/createLeave", Talep("2026-09-01", "2026-09-05", "Mazeret"));

        HttpResponseMessage response = await calisan.PostAsync("/Leave/createLeave",
            Talep(bas, bit, "Mazeret"));

        response.StatusCode.Should().Be(HttpStatusCode.OK, $"senaryo: {senaryo} - cakisma yok");
    }

    [Fact]
    public async Task ReddedilmisTalep_CakismaSayilmaz()
    {
        ApiClient calisan = await Fixture.CalisanClientAsync();
        ApiClient ik = await Fixture.IkClientAsync();

        await calisan.PostAsync("/Leave/createLeave", Talep("2026-09-01", "2026-09-05", "Mazeret"));
        await ik.PutAsync($"/Leave/rejectLeave/{await BekleyenTalepIdAsync(ik)}");

        HttpResponseMessage response = await calisan.PostAsync("/Leave/createLeave",
            Talep("2026-09-01", "2026-09-05", "Mazeret"));

        response.StatusCode.Should().Be(HttpStatusCode.OK,
            "reddedilen talep takvimi mesgul etmemeli");
    }

    // ---------- YERINE BAKACAK KISI ----------

    [Fact]
    public async Task AdayListesi_SadeceAyniDepartmandakiDigerAktifPersoneliDoner()
    {
        ApiClient calisan = await Fixture.CalisanClientAsync();

        List<SubstituteCandidateResponse> adaylar =
            await calisan.GetJsonAsync<List<SubstituteCandidateResponse>>("/Leave/getSubstituteCandidates");

        adaylar.Should().ContainSingle();
        adaylar[0].AdSoyad.Should().Be("Zeynep Aydin");
        adaylar.Should().NotContain(a => a.AdSoyad.Contains("Burak"), "farkli departman");
        adaylar.Should().NotContain(a => a.AdSoyad.Contains("Ahmet"), "kisinin kendisi");
    }

    [Fact]
    public async Task AdayListesi_PasifPersoneliIcermez()
    {
        ApiClient ik = await Fixture.IkClientAsync();
        List<PersonelResponse> hepsi = await ik.GetJsonAsync<List<PersonelResponse>>("/Personnel/getPersonnel");
        int zeynepId = hepsi.Single(p => p.Email == TestUsers.MeslektasEmail).Id;
        await ik.DeleteAsync($"/Personnel/deletePersonnel/{zeynepId}");

        ApiClient calisan = await Fixture.CalisanClientAsync();
        List<SubstituteCandidateResponse> adaylar =
            await calisan.GetJsonAsync<List<SubstituteCandidateResponse>>("/Leave/getSubstituteCandidates");

        adaylar.Should().BeEmpty("isten ayrilan kisi yerine bakamaz");
    }

    [Fact]
    public async Task GecerliAday_KabulEdilir_VeIsimTaleptteGorunur()
    {
        ApiClient ik = await Fixture.IkClientAsync();
        List<PersonelResponse> hepsi = await ik.GetJsonAsync<List<PersonelResponse>>("/Personnel/getPersonnel");
        int zeynepId = hepsi.Single(p => p.Email == TestUsers.MeslektasEmail).Id;

        ApiClient calisan = await Fixture.CalisanClientAsync();
        HttpResponseMessage response = await calisan.PostAsync("/Leave/createLeave",
            Talep("2026-09-01", "2026-09-03", "Yillik", zeynepId));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        List<IzinTalepResponse> talepler = await calisan.GetJsonAsync<List<IzinTalepResponse>>("/Leave/getMyLeaves");
        talepler.Single().SubstituteAdSoyad.Should().Be("Zeynep Aydin");
    }

    [Fact]
    public async Task FarkliDepartmandanAday_Reddedilir()
    {
        ApiClient ik = await Fixture.IkClientAsync();
        List<PersonelResponse> hepsi = await ik.GetJsonAsync<List<PersonelResponse>>("/Personnel/getPersonnel");
        int burakId = hepsi.Single(p => p.Email == TestUsers.BaskaDepartmanEmail).Id;

        ApiClient calisan = await Fixture.CalisanClientAsync();
        HttpResponseMessage response = await calisan.PostAsync("/Leave/createLeave",
            Talep("2026-09-01", "2026-09-03", "Yillik", burakId));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("aynı departmandan");
    }

    [Fact]
    public async Task KisiKendisiniSecemez()
    {
        ApiClient calisan = await Fixture.CalisanClientAsync();
        ApiClient ik = await Fixture.IkClientAsync();
        List<PersonelResponse> hepsi = await ik.GetJsonAsync<List<PersonelResponse>>("/Personnel/getPersonnel");
        int kendiId = hepsi.Single(p => p.Email == TestUsers.CalisanEmail).Id;

        HttpResponseMessage response = await calisan.PostAsync("/Leave/createLeave",
            Talep("2026-09-01", "2026-09-03", "Yillik", kendiId));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Kendinizi");
    }

    [Fact]
    public async Task OlmayanAdayId_Reddedilir()
    {
        ApiClient calisan = await Fixture.CalisanClientAsync();

        HttpResponseMessage response = await calisan.PostAsync("/Leave/createLeave",
            Talep("2026-09-01", "2026-09-03", "Yillik", 999999));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task YerineBakanIstenAyrilirsa_BekleyenTalepteBagKopar_TalepSilinmez()
    {
        ApiClient ik = await Fixture.IkClientAsync();
        List<PersonelResponse> hepsi = await ik.GetJsonAsync<List<PersonelResponse>>("/Personnel/getPersonnel");
        int zeynepId = hepsi.Single(p => p.Email == TestUsers.MeslektasEmail).Id;

        ApiClient calisan = await Fixture.CalisanClientAsync();
        await calisan.PostAsync("/Leave/createLeave", Talep("2026-09-01", "2026-09-03", "Yillik", zeynepId));

        await ik.DeleteAsync($"/Personnel/deletePersonnel/{zeynepId}");

        List<IzinTalepResponse> talepler = await calisan.GetJsonAsync<List<IzinTalepResponse>>("/Leave/getMyLeaves");
        talepler.Should().ContainSingle("talep silinmemeli");
        talepler.Single().SubstituteId.Should()
            .BeNull("isten ayrilan kisi bekleyen talepte yerine bakan olarak kalmamali");
    }

    [Fact]
    public async Task YerineBakanIstenAyrilirsa_OnaylanmisTaleptekiGecmisKorunur()
    {
        ApiClient ik = await Fixture.IkClientAsync();
        List<PersonelResponse> hepsi = await ik.GetJsonAsync<List<PersonelResponse>>("/Personnel/getPersonnel");
        int zeynepId = hepsi.Single(p => p.Email == TestUsers.MeslektasEmail).Id;

        ApiClient calisan = await Fixture.CalisanClientAsync();
        await calisan.PostAsync("/Leave/createLeave", Talep("2026-09-01", "2026-09-03", "Yillik", zeynepId));
        await ik.PutAsync($"/Leave/approveLeave/{await BekleyenTalepIdAsync(ik)}");

        await ik.DeleteAsync($"/Personnel/deletePersonnel/{zeynepId}");

        List<IzinTalepResponse> talepler = await calisan.GetJsonAsync<List<IzinTalepResponse>>("/Leave/getMyLeaves");
        talepler.Single().SubstituteAdSoyad.Should()
            .Be("Zeynep Aydin", "onaylanmis izin tarihsel kayittir, degistirilmemeli");
    }

    // ---------- ONAY SINIR DURUMLARI ----------

    [Fact]
    public async Task AyniTalep_IkinciKezOnaylanamaz_CifteDusumOnlenir()
    {
        ApiClient calisan = await Fixture.CalisanClientAsync();
        ApiClient ik = await Fixture.IkClientAsync();

        await calisan.PostAsync("/Leave/createLeave", Talep("2026-09-01", "2026-09-03"));
        int talepId = await BekleyenTalepIdAsync(ik);

        (await ik.PutAsync($"/Leave/approveLeave/{talepId}")).StatusCode.Should().Be(HttpStatusCode.OK);

        HttpResponseMessage ikinci = await ik.PutAsync($"/Leave/approveLeave/{talepId}");
        ikinci.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        IzinOzetResponse ozet = await calisan.GetJsonAsync<IzinOzetResponse>("/Leave/getMySummary");
        ozet.KullanilanGun.Should().Be(3, "gun sayisi iki kez dusmemeli");
    }

    [Fact]
    public async Task OnaylanmisTalep_Reddedilemez()
    {
        ApiClient calisan = await Fixture.CalisanClientAsync();
        ApiClient ik = await Fixture.IkClientAsync();

        await calisan.PostAsync("/Leave/createLeave", Talep("2026-09-01", "2026-09-03"));
        int talepId = await BekleyenTalepIdAsync(ik);
        await ik.PutAsync($"/Leave/approveLeave/{talepId}");

        (await ik.PutAsync($"/Leave/rejectLeave/{talepId}")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task OlmayanTalep_Onaylanamaz()
    {
        ApiClient ik = await Fixture.IkClientAsync();

        (await ik.PutAsync("/Leave/approveLeave/999999")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// YARIS DURUMU: Ayni talep iki es zamanli istekle onaylanmaya calisilir.
    /// Beklenen: yalnizca biri basarili olur ve bakiyeden tek kez dusulur.
    /// </summary>
    [Fact]
    public async Task EsZamanliIkiOnay_BakiyedenSadeceBirKezDuser()
    {
        ApiClient calisan = await Fixture.CalisanClientAsync();
        ApiClient ik = await Fixture.IkClientAsync();

        await calisan.PostAsync("/Leave/createLeave", Talep("2026-09-01", "2026-09-05"));
        int talepId = await BekleyenTalepIdAsync(ik);

        ApiClient ik1 = await Fixture.IkClientAsync();
        ApiClient ik2 = await Fixture.IkClientAsync();

        Task<HttpResponseMessage>[] istekler =
        {
            ik1.PutAsync($"/Leave/approveLeave/{talepId}"),
            ik2.PutAsync($"/Leave/approveLeave/{talepId}")
        };
        HttpResponseMessage[] sonuclar = await Task.WhenAll(istekler);

        sonuclar.Count(r => r.StatusCode == HttpStatusCode.OK)
            .Should().Be(1, "ayni talep yalnizca bir kez onaylanabilmeli");

        IzinOzetResponse ozet = await calisan.GetJsonAsync<IzinOzetResponse>("/Leave/getMySummary");
        ozet.KullanilanGun.Should().Be(5, "es zamanli onay bakiyeyi iki kez dusurmemeli");
    }

    // ---------- YETKI ----------

    [Fact]
    public async Task Calisan_BaskaPersonelinIzinGecmisineErisemez()
    {
        ApiClient calisan = await Fixture.CalisanClientAsync();

        (await calisan.GetAsync("/Leave/getPersonnelHistory/2")).StatusCode
            .Should().Be(HttpStatusCode.Forbidden);
        (await calisan.GetAsync("/Leave/getLeaves")).StatusCode
            .Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Ik_IzinTalebiOlusturamaz_AkisSadeceCalisanaAit()
    {
        ApiClient ik = await Fixture.IkClientAsync();

        (await ik.PostAsync("/Leave/createLeave", Talep("2026-09-01", "2026-09-03")))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Calisan_SadeceKendiTaleplerinigorur()
    {
        ApiClient ahmet = await Fixture.CalisanClientAsync();
        ApiClient zeynep = await Fixture.MeslektasClientAsync();

        await ahmet.PostAsync("/Leave/createLeave", Talep("2026-09-01", "2026-09-03"));

        List<IzinTalepResponse> zeynepinListesi =
            await zeynep.GetJsonAsync<List<IzinTalepResponse>>("/Leave/getMyLeaves");

        zeynepinListesi.Should().BeEmpty("baskasinin talebi gorunmemeli");
    }
}
