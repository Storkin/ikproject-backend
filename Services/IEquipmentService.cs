using IkProjesi.DTOs;

namespace IkProjesi.Services;

public interface IEquipmentService
{
    Task<List<ZimmetDto>> GetAllAsync();
    Task<List<ZimmetDto>> GetByPersonnelIdAsync(int personnelId);
    Task<(bool success, string message)> AssignAsync(ZimmetOlusturDto dto);
    Task<(bool success, string message)> ReturnAsync(int id);
    Task<(bool success, string message)> DeleteAsync(int id);
}
