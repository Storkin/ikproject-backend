using IkProjesi.Models;

namespace IkProjesi.Repositories;

public interface IEducationRepository
{
    Task<List<Egitim>> GetByPersonnelIdAsync(int personnelId);
    Task<Egitim> GetByIdAsync(int id);
    Task AddAsync(Egitim education);
    Task UpdateAsync(Egitim education);
    Task DeleteAsync(Egitim education);
}
