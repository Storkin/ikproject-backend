using IkProjesi.DTOs;

namespace IkProjesi.Services;

public interface IPersonelService
{
    Task<List<PersonelDto>> GetAllAsync();
    Task<List<PersonelDto>> GetByDepartmanAsync(string departman);
    Task<List<PersonelDto>> GetOrderedByMaasAsync(bool azalan);
    Task<PersonelDto?> GetByIdAsync(int id);
    Task<PersonelDto?> GetByEmailAsync(string email);
    Task<List<PersonelDto>> AraAsync(string kelime);
    Task<PersonelDto> AddAsync(PersonelCreateDto dto);
    Task<PersonelDto?> UpdateAsync(int id, PersonelUpdateDto dto);
    Task<bool> UpdateEmailAsync(int id, CalisanEmailUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}
