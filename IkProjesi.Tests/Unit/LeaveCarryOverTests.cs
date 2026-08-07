using FluentAssertions;
using IkProjesi.DTOs;
using IkProjesi.Models;
using IkProjesi.Repositories;
using IkProjesi.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace IkProjesi.Tests.Unit;

/// <summary>
/// Yil devri mantigi. Entegrasyon testiyle sinanamaz cunku sistem saatini
/// ileri alamayiz; bu yuzden repository sahtelenip gecmis yil kayitlari kurgulanir.
/// </summary>
public class LeaveCarryOverTests
{
    private const int VarsayilanHak = 14;
    private static int BuYil => DateTime.UtcNow.Year;

    private readonly Mock<ILeaveRepository> leaveRepo = new();
    private readonly Mock<IPersonnelRepository> personnelRepo = new();
    private readonly Mock<ILeaveBalanceRepository> balanceRepo = new();
    private readonly List<IzinHakki> yazilanKayitlar = new();

    private LeaveService OlusturServis(params IzinHakki[] mevcutKayitlar)
    {
        List<IzinHakki> depo = mevcutKayitlar.ToList();

        balanceRepo.Setup(r => r.GetByPersonnelAndYearAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((int pid, int yil) => depo.FirstOrDefault(h => h.PersonelId == pid && h.Yil == yil)!);

        balanceRepo.Setup(r => r.GetLatestAsync(It.IsAny<int>()))
            .ReturnsAsync((int pid) => depo.Where(h => h.PersonelId == pid)
                                           .OrderByDescending(h => h.Yil)
                                           .FirstOrDefault()!);

        balanceRepo.Setup(r => r.GetByPersonnelIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int pid) => depo.Where(h => h.PersonelId == pid)
                                           .OrderByDescending(h => h.Yil)
                                           .ToList());

        balanceRepo.Setup(r => r.AddAsync(It.IsAny<IzinHakki>()))
            .Callback((IzinHakki h) => { depo.Add(h); yazilanKayitlar.Add(h); })
            .Returns(Task.CompletedTask);

        balanceRepo.Setup(r => r.UpdateAsync(It.IsAny<IzinHakki>())).Returns(Task.CompletedTask);

        personnelRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Personel { Id = 1, Ad = "Test", Soyad = "Kisi", Departman = Departman.Muhasebe });

        leaveRepo.Setup(r => r.GetByPersonnelIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<IzinTalep>());

        leaveRepo.Setup(r => r.GetOverlappingAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync((IzinTalep?)null);

        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PersonelAyarlari:VarsayilanIzinHakki"] = VarsayilanHak.ToString()
            })
            .Build();

        return new LeaveService(leaveRepo.Object, personnelRepo.Object, balanceRepo.Object, config);
    }

    private static IzinHakki Kayit(int yil, int hakEdilen = VarsayilanHak, int devreden = 0,
        int kullanilan = 0) =>
        new()
        {
            PersonelId = 1,
            Yil = yil,
            HakEdilen = hakEdilen,
            Devreden = devreden,
            Kullanilan = kullanilan
        };

    [Fact]
    public async Task HicKaydiYoksa_BuYilIcinSifirdanOlusturulur()
    {
        LeaveService servis = OlusturServis();

        IzinOzetDto? ozet = await servis.GetSummaryAsync(1);

        ozet!.Yil.Should().Be(BuYil);
        ozet.HakEdilenGun.Should().Be(VarsayilanHak);
        ozet.DevredenGun.Should().Be(0);
        ozet.KalanGun.Should().Be(VarsayilanHak);
    }

    [Fact]
    public async Task GecenYildanKalanGunler_BuYilaDevreder()
    {
        // Gecen yil: 14 hak, 4 kullanilmis -> 10 devretmeli
        LeaveService servis = OlusturServis(Kayit(BuYil - 1, kullanilan: 4));

        IzinOzetDto? ozet = await servis.GetSummaryAsync(1);

        ozet!.Yil.Should().Be(BuYil);
        ozet.DevredenGun.Should().Be(10);
        ozet.HakEdilenGun.Should().Be(14);
        ozet.ToplamHak.Should().Be(24);
        ozet.KullanilanGun.Should().Be(0, "yeni yilda kullanim sifirlanir");
        ozet.KalanGun.Should().Be(24);
    }

    [Fact]
    public async Task GecenYilTamamenKullanilmissa_DevirSifirdir()
    {
        LeaveService servis = OlusturServis(Kayit(BuYil - 1, kullanilan: 14));

        IzinOzetDto? ozet = await servis.GetSummaryAsync(1);

        ozet!.DevredenGun.Should().Be(0);
        ozet.ToplamHak.Should().Be(14);
    }

    [Fact]
    public async Task AradaBirdenFazlaYilVarsa_HerYilIcinKayitUretilir_ZincirBozulmaz()
    {
        // 3 yil once: 14 hak, 4 kullanilmis -> 10 kalan
        LeaveService servis = OlusturServis(Kayit(BuYil - 3, kullanilan: 4));

        List<IzinHakkiDto> dokum = await servis.GetBalanceHistoryAsync(1);

        dokum.Should().HaveCount(4, "eksik yillar da doldurulmali");

        IzinHakkiDto ikiYilOnce = dokum.Single(d => d.Yil == BuYil - 2);
        IzinHakkiDto birYilOnce = dokum.Single(d => d.Yil == BuYil - 1);
        IzinHakkiDto buYil = dokum.Single(d => d.Yil == BuYil);

        ikiYilOnce.DevredenGun.Should().Be(10);
        ikiYilOnce.ToplamHak.Should().Be(24);

        birYilOnce.DevredenGun.Should().Be(24);
        birYilOnce.ToplamHak.Should().Be(38);

        buYil.DevredenGun.Should().Be(38);
        buYil.ToplamHak.Should().Be(52);
    }

    [Fact]
    public async Task BuYilinKaydiVarsa_TekrarOlusturulmaz()
    {
        LeaveService servis = OlusturServis(Kayit(BuYil, kullanilan: 3));

        await servis.GetSummaryAsync(1);

        yazilanKayitlar.Should().BeEmpty("mevcut yil kaydi varken yenisi yazilmamali");
        balanceRepo.Verify(r => r.AddAsync(It.IsAny<IzinHakki>()), Times.Never);
    }

    [Fact]
    public async Task DevredenBakiye_YeniTaleplerdeKullanilabilir()
    {
        LeaveService servis = OlusturServis(Kayit(BuYil - 1, kullanilan: 0)); // 14 devredecek

        // Toplam 28 gun hak olmali; 20 gunluk talep gecmeli
        (bool success, string message) sonuc = await servis.CreateRequestAsync(1, new IzinTalepOlusturDto
        {
            BaslangicTarihi = new DateTime(BuYil, 6, 1),
            BitisTarihi = new DateTime(BuYil, 6, 20),
            Turu = IzinTuru.Yillik
        });

        sonuc.success.Should().BeTrue(sonuc.message);
    }

    [Fact]
    public async Task DevirDahilBakiyeyiAsanTalep_Reddedilir()
    {
        LeaveService servis = OlusturServis(Kayit(BuYil - 1, kullanilan: 0)); // toplam 28

        (bool success, string message) sonuc = await servis.CreateRequestAsync(1, new IzinTalepOlusturDto
        {
            BaslangicTarihi = new DateTime(BuYil, 6, 1),
            BitisTarihi = new DateTime(BuYil, 7, 15), // 45 gun
            Turu = IzinTuru.Yillik
        });

        sonuc.success.Should().BeFalse();
        sonuc.message.Should().Contain("Yetersiz izin hakkı");
    }

    [Fact]
    public async Task NegatifKalanOlusursa_DevirSifiraSabitlenir()
    {
        // Veri tutarsizligi: kullanilan, toplam hakki asmis
        LeaveService servis = OlusturServis(Kayit(BuYil - 1, kullanilan: 20));

        IzinOzetDto? ozet = await servis.GetSummaryAsync(1);

        ozet!.DevredenGun.Should().Be(0, "negatif devir olmamali");
    }
}
