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

    public async Task<List<Personel>> GetAllAsync()
    {
        List<Personel> allPersonnel = await db.Personeller
            .Include(p => p.Experiences)
            .ToListAsync();

        return allPersonnel;
    }

    public async Task<List<Personel>> GetByDepartmentAsync(Departman department)
    {
        List<Personel> sameDepartment = await db.Personeller
            .Include(p => p.Experiences)
            .Where(p => p.Departman == department)
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
                .OrderByDescending(p => p.Maas)
                .ToListAsync();
        }
        else
        {
            sortedList = await db.Personeller
                .Include(p => p.Experiences)
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
            .Where(p => p.Ad.ToLower().Contains(lowerKeyword) ||
                        p.Soyad.ToLower().Contains(lowerKeyword))
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
