using IkProjesi.Models;

namespace IkProjesi.Repositories;

public interface ISalaryRepository
{
    Task<List<MaasKaydi>> GetAllAsync();
    Task<List<MaasKaydi>> GetByPersonnelIdAsync(int personnelId);
    Task<MaasKaydi> GetByIdAsync(int id);
    Task AddAsync(MaasKaydi record);
    Task DeleteAsync(MaasKaydi record);
}
