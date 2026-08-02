using IkProjesi.Models;

namespace IkProjesi.Repositories;

public interface IExperienceRepository
{
    Task<List<Experience>> GetByPersonnelIdAsync(int personnelId);
    Task ReplaceForPersonnelAsync(int personnelId, List<Experience> experiences);
}
