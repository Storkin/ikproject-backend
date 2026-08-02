using IkProjesi.DTOs;

namespace IkProjesi.Services;

public interface ISalaryService
{
    Task<List<MaasKaydiDto>> GetByPersonnelIdAsync(int personnelId);
    Task<(bool success, string message)> AddAsync(MaasKaydiOlusturDto dto);
    Task<(bool success, string message)> DeleteAsync(int id);
}
