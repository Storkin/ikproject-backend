using IkProjesi.Data;
using IkProjesi.Models;
using Microsoft.EntityFrameworkCore;

namespace IkProjesi.Repositories;

public class PersonelRepository : IPersonelRepository
{
    private readonly AppDbContext db;

    public PersonelRepository(AppDbContext context)
    {
        db = context;
    }

    public async Task<List<Personel>> GetAllAsync()
    {
        List<Personel> tumPersoneller = await db.Personeller.ToListAsync();
        return tumPersoneller;
    }

    public async Task<List<Personel>> GetByDepartmanAsync(string departman)
    {
        List<Personel> ayniDepartmandakiler = await db.Personeller
            .Where(p => p.Departman == departman)
            .ToListAsync();

        return ayniDepartmandakiler;
    }

    public async Task<List<Personel>> GetOrderedByMaasAsync(bool azalan)
    {
        List<Personel> siraliListe;

        if (azalan == true)
        {
            siraliListe = await db.Personeller
                .OrderByDescending(p => p.Maas)
                .ToListAsync();
        }
        else
        {
            siraliListe = await db.Personeller
                .OrderBy(p => p.Maas)
                .ToListAsync();
        }

        return siraliListe;
    }

    public async Task<List<Personel>> AraAsync(string kelime)
    {
        string kucukHarfKelime = kelime.ToLower();

        List<Personel> bulunanlar = await db.Personeller
            .Where(p => p.Ad.ToLower().Contains(kucukHarfKelime) ||
                        p.Soyad.ToLower().Contains(kucukHarfKelime))
            .ToListAsync();

        return bulunanlar;
    }

    public async Task<Personel> GetByIdAsync(int id)
    {
        Personel bulunanPersonel = await db.Personeller.FindAsync(id);
        return bulunanPersonel;
    }

    public async Task<Personel> GetByEmailAsync(string email)
    {
        Personel bulunanPersonel = await db.Personeller
            .FirstOrDefaultAsync(p => p.Email == email);

        return bulunanPersonel;
    }

    public async Task AddAsync(Personel personel)
    {
        await db.Personeller.AddAsync(personel);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Personel personel)
    {
        db.Personeller.Update(personel);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Personel personel)
    {
        db.Personeller.Remove(personel);
        await db.SaveChangesAsync();
    }
}
