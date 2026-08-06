using IkProjesi.Models;

namespace IkProjesi.Repositories;

public interface IEquipmentRepository
{
    Task<List<Zimmet>> GetAllAsync();
    Task<List<Zimmet>> GetByPersonnelIdAsync(int personnelId);
    Task<Zimmet> GetByIdAsync(int id);
    Task AddAsync(Zimmet equipment);
    Task UpdateAsync(Zimmet equipment);
    Task DeleteAsync(Zimmet equipment);
}
