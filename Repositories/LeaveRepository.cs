using IkProjesi.Data;
using IkProjesi.Models;
using Microsoft.EntityFrameworkCore;

namespace IkProjesi.Repositories;

public class LeaveRepository : ILeaveRepository
{
    private readonly AppDbContext db;

    public LeaveRepository(AppDbContext context)
    {
        db = context;
    }

    public async Task<List<IzinTalep>> GetAllAsync()
    {
        List<IzinTalep> allRequests = await db.IzinTalepler
            .Include(t => t.Personel)
            .Include(t => t.Substitute)
            .ToListAsync();

        return allRequests;
    }

    public async Task<List<IzinTalep>> GetPendingAsync()
    {
        List<IzinTalep> pendingRequests = await db.IzinTalepler
            .Include(t => t.Personel)
            .Include(t => t.Substitute)
            .Where(t => t.Durum == IzinDurum.Beklemede)
            .ToListAsync();

        return pendingRequests;
    }

    public async Task<List<IzinTalep>> GetByPersonnelIdAsync(int personnelId)
    {
        List<IzinTalep> personnelRequests = await db.IzinTalepler
            .Include(t => t.Personel)
            .Include(t => t.Substitute)
            .Where(t => t.PersonelId == personnelId)
            .ToListAsync();

        return personnelRequests;
    }

    public async Task<IzinTalep> GetByIdAsync(int id)
    {
        IzinTalep found = await db.IzinTalepler
            .Include(t => t.Personel)
            .Include(t => t.Substitute)
            .FirstOrDefaultAsync(t => t.Id == id);

        return found;
    }

    public async Task AddAsync(IzinTalep request)
    {
        await db.IzinTalepler.AddAsync(request);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(IzinTalep request)
    {
        db.IzinTalepler.Update(request);
        await db.SaveChangesAsync();
    }
}
