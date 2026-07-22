using IkProjesi.DTOs;
using IkProjesi.Models;
using IkProjesi.Repositories;

namespace IkProjesi.Services;

public class DuyuruService : IDuyuruService
{
    private readonly IDuyuruRepository duyuruDepo;

    public DuyuruService(IDuyuruRepository repository)
    {
        duyuruDepo = repository;
    }

    public async Task<List<DuyuruDto>> GetAllAsync()
    {
        List<Duyuru> tumDuyurular = await duyuruDepo.GetAllAsync();

        List<DuyuruDto> sonucListesi = new List<DuyuruDto>();
        foreach (Duyuru duyuru in tumDuyurular)
        {
            DuyuruDto dto = DuyurudanDtoYap(duyuru);
            sonucListesi.Add(dto);
        }

        return sonucListesi;
    }

    public async Task <DuyuruDto> AddAsync(DuyuruOlusturDto dto)
    {
        Duyuru yeniDuyuru = new Duyuru();
        yeniDuyuru.Baslik = dto.Baslik;
        yeniDuyuru.Icerik = dto.Icerik;

        await duyuruDepo.AddAsync(yeniDuyuru);
        return DuyurudanDtoYap(yeniDuyuru);
    }


    public async Task<DuyuruDto?> UpdateAsync(int id, DuyuruUpdateDto dto)
    {
        Duyuru guncellenecekDuyuru = await duyuruDepo.GetByIdAsync(id);

        if (guncellenecekDuyuru == null)
        {
            return null;
        }

        guncellenecekDuyuru.Baslik = dto.Baslik;
        guncellenecekDuyuru.Icerik = dto.Icerik;

        await duyuruDepo.UpdateAsync(guncellenecekDuyuru);

        DuyuruDto sonuc = DuyurudanDtoYap(guncellenecekDuyuru);
        return sonuc;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        Duyuru silinecekDuyuru = await duyuruDepo.GetByIdAsync(id);

        if (silinecekDuyuru == null)
        {
            return false;
        }

        await duyuruDepo.DeleteAsync(silinecekDuyuru);
        return true;
    }

    private DuyuruDto DuyurudanDtoYap(Duyuru duyuru)
    {
        DuyuruDto dto = new DuyuruDto();
        dto.Id = duyuru.Id;
        dto.Baslik = duyuru.Baslik;
        dto.Icerik = duyuru.Icerik;
        dto.YayinTarihi = duyuru.YayinTarihi;
        return dto;
    }
}
