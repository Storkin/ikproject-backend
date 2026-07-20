using IkProjesi.Models;

namespace IkProjesi.Repositories;

public interface IPersonelRepository
{
    Task<List<Personel>> GetAllAsync();
    Task<List<Personel>> GetByDepartmanAsync(string departman);
    Task<List<Personel>> GetOrderedByMaasAsync(bool azalan);
    Task<Personel?> GetByIdAsync(int id);
    Task<Personel?> GetByEmailAsync(string email);
    Task<List<Personel>> AraAsync(string kelime);
    Task AddAsync(Personel personel);
    Task UpdateAsync(Personel personel);
    Task DeleteAsync(Personel personel);
}
