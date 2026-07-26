using IkProjesi.DTOs;

namespace IkProjesi.Services;

public interface IAnnouncementService
{
    Task<List<DuyuruDto>> GetAllAsync();
    Task<DuyuruDto> AddAsync(DuyuruOlusturDto dto);
    Task<DuyuruDto?> UpdateAsync(int id, DuyuruUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}
