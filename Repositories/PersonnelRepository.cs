using IkProjesi.Data;
using IkProjesi.Models;
using Microsoft.EntityFrameworkCore;

namespace IkProjesi.Repositories;

public class PersonnelRepository : IPersonnelRepository
{
    private readonly AppDbContext db;

    public PersonnelRepository(AppDbContext context)
    {
        db = context;
    }

    // Isten ayrilan personel silinmez, pasife alinir.
    // Listelerde varsayilan olarak sadece aktif calisanlar doner.
    public async Task<List<Personel>> GetAllAsync(bool includeInactive = false)
    {
        IQueryable<Personel> query = db.Personeller.Include(p => p.Experiences);

        if (includeInactive == false)
        {
            query = query.Where(p => p.AktifMi);
        }

        List<Personel> allPersonnel = await query.ToListAsync();
        return allPersonnel;
    }

    public async Task<List<Personel>> GetByDepartmentAsync(Departman department)
    {
        List<Personel> sameDepartment = await db.Personeller
            .Include(p => p.Experiences)
            .Where(p => p.Departman == department && p.AktifMi)
            .ToListAsync();

        return sameDepartment;
    }

    public async Task<List<Personel>> GetOrderedBySalaryAsync(bool descending)
    {
        List<Personel> sortedList;

        if (descending == true)
        {
            sortedList = await db.Personeller
                .Include(p => p.Experiences)
                .Where(p => p.AktifMi)
                .OrderByDescending(p => p.Maas)
                .ToListAsync();
        }
        else
        {
            sortedList = await db.Personeller
                .Include(p => p.Experiences)
                .Where(p => p.AktifMi)
                .OrderBy(p => p.Maas)
                .ToListAsync();
        }

        return sortedList;
    }

    public async Task<List<Personel>> SearchAsync(string keyword)
    {
        string lowerKeyword = keyword.ToLower();

        List<Personel> found = await db.Personeller
            .Include(p => p.Experiences)
            .Where(p => p.AktifMi &&
                        (p.Ad.ToLower().Contains(lowerKeyword) ||
                         p.Soyad.ToLower().Contains(lowerKeyword)))
            .ToListAsync();

        return found;
    }

    public async Task<Personel> GetByIdAsync(int id)
    {
        Personel found = await db.Personeller
            .Include(p => p.Experiences)
            .FirstOrDefaultAsync(p => p.Id == id);

        return found;
    }

    public async Task<Personel> GetByEmailAsync(string email)
    {
        Personel found = await db.Personeller
            .Include(p => p.Experiences)
            .FirstOrDefaultAsync(p => p.Email == email);

        return found;
    }

    public async Task AddAsync(Personel personnel)
    {
        await db.Personeller.AddAsync(personnel);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Personel personnel)
    {
        db.Personeller.Update(personnel);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Personel personnel)
    {
        db.Personeller.Remove(personnel);
        await db.SaveChangesAsync();
    }
}
