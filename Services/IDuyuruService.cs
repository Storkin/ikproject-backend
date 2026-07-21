using IkProjesi.DTOs;

namespace IkProjesi.Services;

public interface IDuyuruService
{
    Task<List<DuyuruDto>> GetAllAsync();
    Task <DuyuruDto> AddAsync(DuyuruOlusturDto dto);
    Task<bool> DeleteAsync(int id);
}
