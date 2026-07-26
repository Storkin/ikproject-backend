using IkProjesi.Models;

namespace IkProjesi.Repositories;

public interface IPersonnelRepository
{
    Task<List<Personel>> GetAllAsync();
    Task<List<Personel>> GetByDepartmentAsync(string department);
    Task<List<Personel>> GetOrderedBySalaryAsync(bool descending);
    Task<Personel> GetByIdAsync(int id);
    Task<Personel> GetByEmailAsync(string email);
    Task<List<Personel>> SearchAsync(string keyword);
    Task AddAsync(Personel personnel);
    Task UpdateAsync(Personel personnel);
    Task DeleteAsync(Personel personnel);
}
