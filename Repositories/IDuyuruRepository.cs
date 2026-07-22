using IkProjesi.Models;

namespace IkProjesi.Repositories;

public interface IDuyuruRepository
{
    Task<List<Duyuru>> GetAllAsync();
    Task<Duyuru?> GetByIdAsync(int id);
    Task AddAsync(Duyuru duyuru);
    Task UpdateAsync(Duyuru duyuru);
    Task DeleteAsync(Duyuru duyuru);
}
