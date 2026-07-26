using IkProjesi.Data;
using IkProjesi.Models;
using Microsoft.EntityFrameworkCore;

namespace IkProjesi.Repositories;

public class AnnouncementRepository : IAnnouncementRepository
{
    private readonly AppDbContext db;

    public AnnouncementRepository(AppDbContext context)
    {
        db = context;
    }

    public async Task<List<Duyuru>> GetAllAsync()
    {
        List<Duyuru> allAnnouncements = await db.Duyurular
            .OrderByDescending(d => d.YayinTarihi)
            .ToListAsync();

        return allAnnouncements;
    }

    public async Task<Duyuru> GetByIdAsync(int id)
    {
        Duyuru found = await db.Duyurular.FindAsync(id);
        return found;
    }

    public async Task AddAsync(Duyuru announcement)
    {
        await db.Duyurular.AddAsync(announcement);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Duyuru announcement)
    {
        db.Duyurular.Update(announcement);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Duyuru announcement)
    {
        db.Duyurular.Remove(announcement);
        await db.SaveChangesAsync();
    }
}
