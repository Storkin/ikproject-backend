using IkProjesi.Data;
using IkProjesi.Models;
using Microsoft.EntityFrameworkCore;

namespace IkProjesi.Repositories;

public class LeaveBalanceRepository : ILeaveBalanceRepository
{
    private readonly AppDbContext db;

    public LeaveBalanceRepository(AppDbContext context)
    {
        db = context;
    }

    public async Task<IzinHakki> GetByPersonnelAndYearAsync(int personnelId, int year)
    {
        IzinHakki found = await db.IzinHaklari
            .FirstOrDefaultAsync(h => h.PersonelId == personnelId && h.Yil == year);

        return found;
    }

    public async Task<IzinHakki> GetLatestAsync(int personnelId)
    {
        IzinHakki found = await db.IzinHaklari
            .Where(h => h.PersonelId == personnelId)
            .OrderByDescending(h => h.Yil)
            .FirstOrDefaultAsync();

        return found;
    }

    public async Task<List<IzinHakki>> GetByPersonnelIdAsync(int personnelId)
    {
        List<IzinHakki> found = await db.IzinHaklari
            .Where(h => h.PersonelId == personnelId)
            .OrderByDescending(h => h.Yil)
            .ToListAsync();

        return found;
    }

    public async Task AddAsync(IzinHakki balance)
    {
        await db.IzinHaklari.AddAsync(balance);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(IzinHakki balance)
    {
        db.IzinHaklari.Update(balance);
        await db.SaveChangesAsync();
    }
}
