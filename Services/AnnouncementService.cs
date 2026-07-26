using IkProjesi.DTOs;
using IkProjesi.Models;
using IkProjesi.Repositories;

namespace IkProjesi.Services;

public class AnnouncementService : IAnnouncementService
{
    private readonly IAnnouncementRepository repo;

    public AnnouncementService(IAnnouncementRepository repository)
    {
        repo = repository;
    }

    public async Task<List<DuyuruDto>> GetAllAsync()
    {
        List<Duyuru> allAnnouncements = await repo.GetAllAsync();

        List<DuyuruDto> resultList = new List<DuyuruDto>();
        foreach (Duyuru announcement in allAnnouncements)
        {
            DuyuruDto dto = MapToDto(announcement);
            resultList.Add(dto);
        }

        return resultList;
    }

    public async Task<DuyuruDto> AddAsync(DuyuruOlusturDto dto)
    {
        Duyuru newAnnouncement = new Duyuru();
        newAnnouncement.Baslik = dto.Baslik;
        newAnnouncement.Icerik = dto.Icerik;

        await repo.AddAsync(newAnnouncement);

        return MapToDto(newAnnouncement);
    }

    public async Task<DuyuruDto?> UpdateAsync(int id, DuyuruUpdateDto dto)
    {
        Duyuru announcement = await repo.GetByIdAsync(id);

        if (announcement == null)
        {
            return null;
        }

        announcement.Baslik = dto.Baslik;
        announcement.Icerik = dto.Icerik;

        await repo.UpdateAsync(announcement);

        DuyuruDto result = MapToDto(announcement);
        return result;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        Duyuru announcement = await repo.GetByIdAsync(id);

        if (announcement == null)
        {
            return false;
        }

        await repo.DeleteAsync(announcement);
        return true;
    }

    private DuyuruDto MapToDto(Duyuru announcement)
    {
        DuyuruDto dto = new DuyuruDto();
        dto.Id = announcement.Id;
        dto.Baslik = announcement.Baslik;
        dto.Icerik = announcement.Icerik;
        dto.YayinTarihi = announcement.YayinTarihi;
        return dto;
    }
}
