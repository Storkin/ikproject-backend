using IkProjesi.Models;

namespace IkProjesi.Repositories;

public interface ILeaveBalanceRepository
{
    Task<IzinHakki> GetByPersonnelAndYearAsync(int personnelId, int year);
    Task<IzinHakki> GetLatestAsync(int personnelId);
    Task<List<IzinHakki>> GetByPersonnelIdAsync(int personnelId);
    Task AddAsync(IzinHakki balance);
    Task UpdateAsync(IzinHakki balance);
}
