using IkProjesi.Data;
using IkProjesi.Models;
using Microsoft.EntityFrameworkCore;

namespace IkProjesi.Repositories;

public class IzinRepository : IIzinRepository
{
    private readonly AppDbContext db;

    public IzinRepository(AppDbContext context)
    {
        db = context;
    }

    public async Task<List<IzinTalep>> GetAllAsync()
    {
        List<IzinTalep> tumTalepler = await db.IzinTalepler
            .Include(t => t.Personel)
            .ToListAsync();

        return tumTalepler;
    }

    public async Task<List<IzinTalep>> GetBekleyenlerAsync()
    {
        List<IzinTalep> bekleyenTalepler = await db.IzinTalepler
            .Include(t => t.Personel)
            .Where(t => t.Durum == IzinDurum.Beklemede)
            .ToListAsync();

        return bekleyenTalepler;
    }

    public async Task<List<IzinTalep>> GetByPersonelIdAsync(int personelId)
    {
        List<IzinTalep> personelTalepleri = await db.IzinTalepler
            .Where(t => t.PersonelId == personelId)
            .ToListAsync();

        return personelTalepleri;
    }

    public async Task<IzinTalep> GetByIdAsync(int id)
    {
        IzinTalep bulunanTalep = await db.IzinTalepler
            .Include(t => t.Personel)
            .FirstOrDefaultAsync(t => t.Id == id);

        return bulunanTalep;
    }

    public async Task AddAsync(IzinTalep talep)
    {
        await db.IzinTalepler.AddAsync(talep);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(IzinTalep talep)
    {
        db.IzinTalepler.Update(talep);
        await db.SaveChangesAsync();
    }
}
