using IkProjesi.Data;
using IkProjesi.Models;
using Microsoft.EntityFrameworkCore;

namespace IkProjesi.Repositories;

public class EducationRepository : IEducationRepository
{
    private readonly AppDbContext db;

    public EducationRepository(AppDbContext context)
    {
        db = context;
    }

    public async Task<List<Egitim>> GetByPersonnelIdAsync(int personnelId)
    {
        List<Egitim> found = await db.Egitimler
            .Include(e => e.Personel)
            .Where(e => e.PersonelId == personnelId)
            .ToListAsync();

        return found;
    }

    public async Task<Egitim> GetByIdAsync(int id)
    {
        Egitim found = await db.Egitimler
            .Include(e => e.Personel)
            .FirstOrDefaultAsync(e => e.Id == id);

        return found;
    }

    public async Task AddAsync(Egitim education)
    {
        await db.Egitimler.AddAsync(education);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Egitim education)
    {
        db.Egitimler.Update(education);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Egitim education)
    {
        db.Egitimler.Remove(education);
        await db.SaveChangesAsync();
    }
}
