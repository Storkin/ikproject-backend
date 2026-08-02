using IkProjesi.Models;

namespace IkProjesi.Repositories;

public interface ILeaveRepository
{
    Task<List<IzinTalep>> GetAllAsync();
    Task<List<IzinTalep>> GetPendingAsync();
    Task<List<IzinTalep>> GetByPersonnelIdAsync(int personnelId);
    Task<IzinTalep> GetByIdAsync(int id);
    Task<IzinTalep?> GetOverlappingAsync(int personnelId, DateTime start, DateTime end);
    Task AddAsync(IzinTalep request);
    Task UpdateAsync(IzinTalep request);
}
