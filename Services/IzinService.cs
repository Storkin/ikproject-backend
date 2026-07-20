using IkProjesi.DTOs;
using IkProjesi.Models;
using IkProjesi.Repositories;

namespace IkProjesi.Services;

public class IzinService : IIzinService
{
    private readonly IIzinRepository izinDepo;
    private readonly IPersonelRepository personelDepo;

    public IzinService(IIzinRepository izinRepository, IPersonelRepository personelRepository)
    {
        izinDepo = izinRepository;
        personelDepo = personelRepository;
    }

    public async Task<List<IzinTalepDto>> GetAllAsync()
    {
        List<IzinTalep> tumTalepler = await izinDepo.GetAllAsync();

        List<IzinTalepDto> sonucListesi = new List<IzinTalepDto>();
        foreach (IzinTalep talep in tumTalepler)
        {
            IzinTalepDto dto = IzindenDtoYap(talep);
            sonucListesi.Add(dto);
        }

        return sonucListesi;
    }

    public async Task<List<IzinTalepDto>> GetBekleyenlerAsync()
    {
        List<IzinTalep> bekleyenler = await izinDepo.GetBekleyenlerAsync();

        List<IzinTalepDto> sonucListesi = new List<IzinTalepDto>();
        foreach (IzinTalep talep in bekleyenler)
        {
            IzinTalepDto dto = IzindenDtoYap(talep);
            sonucListesi.Add(dto);
        }

        return sonucListesi;
    }

    public async Task<List<IzinTalepDto>> GetByPersonelIdAsync(int personelId)
    {
        List<IzinTalep> personelTalepleri = await izinDepo.GetByPersonelIdAsync(personelId);

        List<IzinTalepDto> sonucListesi = new List<IzinTalepDto>();
        foreach (IzinTalep talep in personelTalepleri)
        {
            IzinTalepDto dto = IzindenDtoYap(talep);
            sonucListesi.Add(dto);
        }

        return sonucListesi;
    }

    public async Task<IzinOzetDto?> GetOzetAsync(int personelId)
    {
        Personel personel = await personelDepo.GetByIdAsync(personelId);
        if (personel == null)
        {
            return null;
        }

        List<IzinTalep> tumTalepler = await izinDepo.GetByPersonelIdAsync(personelId);

        List<IzinTalepDto> gecmisListesi = new List<IzinTalepDto>();
        foreach (IzinTalep talep in tumTalepler)
        {
            IzinTalepDto dto = IzindenDtoYap(talep);
            gecmisListesi.Add(dto);
        }

        int kalanIzin = personel.YillikIzinHakki - personel.KullanılanIzin;

        IzinOzetDto ozet = new IzinOzetDto();
        ozet.ToplamHak = personel.YillikIzinHakki;
        ozet.KullanilanGun = personel.KullanılanIzin;
        ozet.KalanGun = kalanIzin;
        ozet.Gecmis = gecmisListesi;

        return ozet;
    }

    public async Task<(bool basarili, string mesaj)> TalepOlusturAsync(int personelId, IzinTalepOlusturDto dto)
    {
        if (dto.BitisTarihi <= dto.BaslangicTarihi)
        {
            return (false, "Bitiş tarihi başlangıç tarihinden sonra olmalı.");
        }

        Personel personel = await personelDepo.GetByIdAsync(personelId);
        if (personel == null)
        {
            return (false, "Personel bulunamadı.");
        }

        int istenenGunSayisi = (dto.BitisTarihi.Date - dto.BaslangicTarihi.Date).Days + 1;
        int kalanIzinHakki = personel.YillikIzinHakki - personel.KullanılanIzin;

        if (istenenGunSayisi > kalanIzinHakki)
        {
            return (false, "Yetersiz izin hakkı. Kalan: " + kalanIzinHakki + " gün.");
        }

        IzinTalep yeniTalep = new IzinTalep();
        yeniTalep.PersonelId = personelId;
        yeniTalep.BaslangicTarihi = dto.BaslangicTarihi;
        yeniTalep.BitisTarihi = dto.BitisTarihi;
        yeniTalep.GunSayisi = istenenGunSayisi;
        yeniTalep.Aciklama = dto.Aciklama;

        await izinDepo.AddAsync(yeniTalep);
        return (true, "İzin talebi oluşturuldu.");
    }

    public async Task<(bool basarili, string mesaj)> OnaylaAsync(int talepId)
    {
        IzinTalep talep = await izinDepo.GetByIdAsync(talepId);
        if (talep == null)
        {
            return (false, "Talep bulunamadı.");
        }

        if (talep.Durum != IzinDurum.Beklemede)
        {
            return (false, "Bu talep zaten işleme alınmış.");
        }

        Personel personel = talep.Personel;
        int kalanIzinHakki = personel.YillikIzinHakki - personel.KullanılanIzin;

        if (talep.GunSayisi > kalanIzinHakki)
        {
            return (false, "Personelin izin hakkı yetmiyor. Kalan: " + kalanIzinHakki + " gün.");
        }

        personel.KullanılanIzin = personel.KullanılanIzin + talep.GunSayisi;
        talep.Durum = IzinDurum.Onaylandi;

        await personelDepo.UpdateAsync(personel);
        await izinDepo.UpdateAsync(talep);
        return (true, "İzin onaylandı.");
    }

    public async Task<(bool basarili, string mesaj)> ReddedAsync(int talepId)
    {
        IzinTalep talep = await izinDepo.GetByIdAsync(talepId);
        if (talep == null)
        {
            return (false, "Talep bulunamadı.");
        }

        if (talep.Durum != IzinDurum.Beklemede)
        {
            return (false, "Bu talep zaten işleme alınmış.");
        }

        talep.Durum = IzinDurum.Reddedildi;
        await izinDepo.UpdateAsync(talep);
        return (true, "İzin reddedildi.");
    }

    private IzinTalepDto IzindenDtoYap(IzinTalep talep)
    {
        string personelAdSoyad = "";
        if (talep.Personel != null)
        {
            personelAdSoyad = talep.Personel.Ad + " " + talep.Personel.Soyad;
        }

        IzinTalepDto dto = new IzinTalepDto();
        dto.Id = talep.Id;
        dto.PersonelId = talep.PersonelId;
        dto.PersonelAdSoyad = personelAdSoyad;
        dto.BaslangicTarihi = talep.BaslangicTarihi;
        dto.BitisTarihi = talep.BitisTarihi;
        dto.GunSayisi = talep.GunSayisi;
        dto.Durum = talep.Durum.ToString();
        dto.TalepTarihi = talep.TalepTarihi;
        dto.Aciklama = talep.Aciklama;
        return dto;
    }
}
