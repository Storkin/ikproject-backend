using IkProjesi.Data;
using IkProjesi.Models;
using Microsoft.EntityFrameworkCore;

namespace IkProjesi.Repositories;

public class ExperienceRepository : IExperienceRepository
{
    private readonly AppDbContext db;

    public ExperienceRepository(AppDbContext context)
    {
        db = context;
    }

    public async Task<List<Experience>> GetByPersonnelIdAsync(int personnelId)
    {
        List<Experience> found = await db.Experiences
            .Where(e => e.PersonelId == personnelId)
            .OrderBy(e => e.Id)
            .ToListAsync();

        return found;
    }

    // Deneyim listesi personel kaydiyla birlikte gonderildigi icin,
    // eski kayitlar silinip gelen liste yeniden yazilir.
    public async Task ReplaceForPersonnelAsync(int personnelId, List<Experience> experiences)
    {
        List<Experience> existing = await db.Experiences
            .Where(e => e.PersonelId == personnelId)
            .ToListAsync();

        db.Experiences.RemoveRange(existing);

        foreach (Experience experience in experiences)
        {
            experience.PersonelId = personnelId;
            await db.Experiences.AddAsync(experience);
        }

        await db.SaveChangesAsync();
    }
}
