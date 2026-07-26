using IkProjesi.DTOs;

namespace IkProjesi.Services;

public interface IPersonnelService
{
    Task<List<PersonelDto>> GetAllAsync();
    Task<List<PersonelDto>> GetByDepartmentAsync(string department);
    Task<List<PersonelDto>> GetOrderedBySalaryAsync(bool descending);
    Task<PersonelDto?> GetByIdAsync(int id);
    Task<PersonelDto?> GetByEmailAsync(string email);
    Task<List<PersonelDto>> SearchAsync(string keyword);
    Task<PersonelDto> AddAsync(PersonelCreateDto dto);
    Task<PersonelDto?> UpdateAsync(int id, PersonelUpdateDto dto);
    Task<bool> UpdateEmailAsync(int id, CalisanEmailUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}
