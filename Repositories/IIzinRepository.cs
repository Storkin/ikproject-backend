using IkProjesi.Models;

namespace IkProjesi.Repositories;

public interface IIzinRepository
{
    Task<List<IzinTalep>> GetAllAsync();
    Task<List<IzinTalep>> GetBekleyenlerAsync();
    Task<List<IzinTalep>> GetByPersonelIdAsync(int personelId);
    Task<IzinTalep?> GetByIdAsync(int id);
    Task AddAsync(IzinTalep talep);
    Task UpdateAsync(IzinTalep talep);
}
