using IkProjesi.Data;
using IkProjesi.Models;
using Microsoft.EntityFrameworkCore;

namespace IkProjesi.Repositories;

public class DuyuruRepository : IDuyuruRepository
{
    private readonly AppDbContext db;

    public DuyuruRepository(AppDbContext context)
    {
        db = context;
    }

    public async Task<List<Duyuru>> GetAllAsync()
    {
        List<Duyuru> tumDuyurular = await db.Duyurular
            .OrderByDescending(d => d.YayinTarihi)
            .ToListAsync();

        return tumDuyurular;
    }

    public async Task<Duyuru> GetByIdAsync(int id)
    {
        Duyuru bulunanDuyuru = await db.Duyurular.FindAsync(id);
        return bulunanDuyuru;
    }

    public async Task AddAsync(Duyuru duyuru)
    {
        await db.Duyurular.AddAsync(duyuru);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Duyuru duyuru)
    {
        db.Duyurular.Remove(duyuru);
        await db.SaveChangesAsync();
    }
}
