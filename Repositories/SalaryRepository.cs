using IkProjesi.Data;
using IkProjesi.Models;
using Microsoft.EntityFrameworkCore;

namespace IkProjesi.Repositories;

public class SalaryRepository : ISalaryRepository
{
    private readonly AppDbContext db;

    public SalaryRepository(AppDbContext context)
    {
        db = context;
    }

    public async Task<List<MaasKaydi>> GetByPersonnelIdAsync(int personnelId)
    {
        List<MaasKaydi> found = await db.MaasKayitlari
            .Include(m => m.Personel)
            .Where(m => m.PersonelId == personnelId)
            .OrderByDescending(m => m.GecerlilikTarihi)
            .ToListAsync();

        return found;
    }

    public async Task<MaasKaydi> GetByIdAsync(int id)
    {
        MaasKaydi found = await db.MaasKayitlari
            .Include(m => m.Personel)
            .FirstOrDefaultAsync(m => m.Id == id);

        return found;
    }

    public async Task AddAsync(MaasKaydi record)
    {
        await db.MaasKayitlari.AddAsync(record);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(MaasKaydi record)
    {
        db.MaasKayitlari.Remove(record);
        await db.SaveChangesAsync();
    }
}
