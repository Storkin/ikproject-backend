using IkProjesi.DTOs;
using IkProjesi.Models;

namespace IkProjesi.Services;

public interface IPersonnelService
{
    Task<List<PersonelDto>> GetAllAsync();
    Task<List<PersonelDto>> GetByDepartmentAsync(Departman department);
    Task<List<PersonelDto>> GetOrderedBySalaryAsync(bool descending);
    Task<PersonelDto?> GetByIdAsync(int id);
    Task<PersonelDto?> GetByEmailAsync(string email);
    Task<List<PersonelDto>> SearchAsync(string keyword);
    Task<PersonelDto> AddAsync(PersonelCreateDto dto);
    Task<PersonelDto?> UpdateAsync(int id, PersonelUpdateDto dto);
    Task<bool> UpdateOwnProfileAsync(int id, CalisanProfilUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}
