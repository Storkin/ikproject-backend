using IkProjesi.DTOs;

namespace IkProjesi.Services;

public interface IEducationService
{
    Task<List<EgitimDto>> GetAllAsync();
    Task<List<EgitimDto>> GetByPersonnelIdAsync(int personnelId);
    Task<(bool success, string message)> AddAsync(EgitimOlusturDto dto);
    Task<(bool success, string message)> UpdateAsync(int id, EgitimGuncelleDto dto);
    Task<(bool success, string message)> DeleteAsync(int id);
}
