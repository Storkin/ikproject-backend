using IkProjesi.DTOs;
using IkProjesi.Models;
using IkProjesi.Repositories;

namespace IkProjesi.Services;

public class PersonelService : IPersonelService
{
    private readonly IPersonelRepository depo;
    private readonly IConfiguration ayarlar;

    public PersonelService(IPersonelRepository repository, IConfiguration config)
    {
        depo = repository;
        ayarlar = config;
    }

    public async Task<List<PersonelDto>> GetAllAsync()
    {
        List<Personel> tumPersoneller = await depo.GetAllAsync();

        List<PersonelDto> sonucListesi = new List<PersonelDto>();
        foreach (Personel personel in tumPersoneller)
        {
            PersonelDto dto = PersoneldenDtoYap(personel);
            sonucListesi.Add(dto);
        }

        return sonucListesi;
    }

    public async Task<List<PersonelDto>> GetByDepartmanAsync(string departman)
    {
        List<Personel> ayniDepartmandakiler = await depo.GetByDepartmanAsync(departman);

        List<PersonelDto> sonucListesi = new List<PersonelDto>();
        foreach (Personel personel in ayniDepartmandakiler)
        {
            PersonelDto dto = PersoneldenDtoYap(personel);
            sonucListesi.Add(dto);
        }

        return sonucListesi;
    }

    public async Task<List<PersonelDto>> GetOrderedByMaasAsync(bool azalan)
    {
        List<Personel> siraliPersoneller = await depo.GetOrderedByMaasAsync(azalan);

        List<PersonelDto> sonucListesi = new List<PersonelDto>();
        foreach (Personel personel in siraliPersoneller)
        {
            PersonelDto dto = PersoneldenDtoYap(personel);
            sonucListesi.Add(dto);
        }

        return sonucListesi;
    }

    public async Task<List<PersonelDto>> AraAsync(string kelime)
    {
        List<Personel> bulunanlar = await depo.AraAsync(kelime);

        List<PersonelDto> sonucListesi = new List<PersonelDto>();
        foreach (Personel personel in bulunanlar)
        {
            PersonelDto dto = PersoneldenDtoYap(personel);
            sonucListesi.Add(dto);
        }

        return sonucListesi;
    }

    public async Task<PersonelDto?> GetByIdAsync(int id)
    {
        Personel bulunanPersonel = await depo.GetByIdAsync(id);

        if (bulunanPersonel == null)
        {
            return null;
        }

        PersonelDto dto = PersoneldenDtoYap(bulunanPersonel);
        return dto;
    }

    public async Task<PersonelDto?> GetByEmailAsync(string email)
    {
        Personel bulunanPersonel = await depo.GetByEmailAsync(email);

        if (bulunanPersonel == null)
        {
            return null;
        }

        PersonelDto dto = PersoneldenDtoYap(bulunanPersonel);
        return dto;
    }

    public async Task<PersonelDto> AddAsync(PersonelCreateDto dto)
    {
        Personel yeniPersonel = new Personel();
        yeniPersonel.Ad = dto.Ad;
        yeniPersonel.Soyad = dto.Soyad;
        yeniPersonel.Departman = dto.Departman;
        yeniPersonel.Maas = dto.Maas;
        yeniPersonel.IseBaslamaTarihi = dto.IseBaslamaTarihi;
        yeniPersonel.Email = dto.Email;
        yeniPersonel.YillikIzinHakki = int.Parse(ayarlar["PersonelAyarlari:VarsayilanIzinHakki"]);

        await depo.AddAsync(yeniPersonel);

        PersonelDto sonuc = PersoneldenDtoYap(yeniPersonel);
        return sonuc;
    }

    public async Task<PersonelDto?> UpdateAsync(int id, PersonelUpdateDto dto)
    {
        Personel guncellenecekPersonel = await depo.GetByIdAsync(id);

        if (guncellenecekPersonel == null)
        {
            return null;
        }

        guncellenecekPersonel.Ad = dto.Ad;
        guncellenecekPersonel.Soyad = dto.Soyad;
        guncellenecekPersonel.Departman = dto.Departman;
        guncellenecekPersonel.Maas = dto.Maas;
        guncellenecekPersonel.Email = dto.Email;

        await depo.UpdateAsync(guncellenecekPersonel);

        PersonelDto sonuc = PersoneldenDtoYap(guncellenecekPersonel);
        return sonuc;
    }

    public async Task<bool> UpdateEmailAsync(int id, CalisanEmailUpdateDto dto)
    {
        Personel guncellenecekPersonel = await depo.GetByIdAsync(id);

        if (guncellenecekPersonel == null)
        {
            return false;
        }

        guncellenecekPersonel.Email = dto.Email;
        await depo.UpdateAsync(guncellenecekPersonel);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        Personel silinecekPersonel = await depo.GetByIdAsync(id);

        if (silinecekPersonel == null)
        {
            return false;
        }

        await depo.DeleteAsync(silinecekPersonel);
        return true;
    }

    private PersonelDto PersoneldenDtoYap(Personel personel)
    {
        PersonelDto dto = new PersonelDto();
        dto.Id = personel.Id;
        dto.Ad = personel.Ad;
        dto.Soyad = personel.Soyad;
        dto.Departman = personel.Departman;
        dto.Maas = personel.Maas;
        dto.IseBaslamaTarihi = personel.IseBaslamaTarihi;
        dto.Email = personel.Email;
        return dto;
    }
}
