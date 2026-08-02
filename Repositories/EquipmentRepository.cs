using IkProjesi.Data;
using IkProjesi.Models;
using Microsoft.EntityFrameworkCore;

namespace IkProjesi.Repositories;

public class EquipmentRepository : IEquipmentRepository
{
    private readonly AppDbContext db;

    public EquipmentRepository(AppDbContext context)
    {
        db = context;
    }

    public async Task<List<Zimmet>> GetByPersonnelIdAsync(int personnelId)
    {
        List<Zimmet> found = await db.Zimmetler
            .Include(z => z.Personel)
            .Where(z => z.PersonelId == personnelId)
            .ToListAsync();

        return found;
    }

    public async Task<Zimmet> GetByIdAsync(int id)
    {
        Zimmet found = await db.Zimmetler
            .Include(z => z.Personel)
            .FirstOrDefaultAsync(z => z.Id == id);

        return found;
    }

    public async Task AddAsync(Zimmet equipment)
    {
        await db.Zimmetler.AddAsync(equipment);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Zimmet equipment)
    {
        db.Zimmetler.Update(equipment);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Zimmet equipment)
    {
        db.Zimmetler.Remove(equipment);
        await db.SaveChangesAsync();
    }
}
