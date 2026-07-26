using IkProjesi.Models;

namespace IkProjesi.Repositories;

public interface IAnnouncementRepository
{
    Task<List<Duyuru>> GetAllAsync();
    Task<Duyuru> GetByIdAsync(int id);
    Task AddAsync(Duyuru announcement);
    Task UpdateAsync(Duyuru announcement);
    Task DeleteAsync(Duyuru announcement);
}
