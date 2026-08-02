using IkProjesi.DTOs;
using IkProjesi.Models;
using IkProjesi.Repositories;

namespace IkProjesi.Services;

public class PersonnelService : IPersonnelService
{
    private readonly IPersonnelRepository repo;
    private readonly IUserRepository userRepo;
    private readonly IExperienceRepository experienceRepo;
    private readonly IConfiguration config;

    public PersonnelService(
        IPersonnelRepository repository,
        IUserRepository userRepository,
        IExperienceRepository experienceRepository,
        IConfiguration configuration)
    {
        repo = repository;
        userRepo = userRepository;
        experienceRepo = experienceRepository;
        config = configuration;
    }

    public async Task<List<PersonelDto>> GetAllAsync()
    {
        List<Personel> allPersonnel = await repo.GetAllAsync();

        List<PersonelDto> resultList = new List<PersonelDto>();
        foreach (Personel personnel in allPersonnel)
        {
            PersonelDto dto = MapToDto(personnel);
            resultList.Add(dto);
        }

        return resultList;
    }

    public async Task<List<PersonelDto>> GetByDepartmentAsync(Departman department)
    {
        List<Personel> sameDepartment = await repo.GetByDepartmentAsync(department);

        List<PersonelDto> resultList = new List<PersonelDto>();
        foreach (Personel personnel in sameDepartment)
        {
            PersonelDto dto = MapToDto(personnel);
            resultList.Add(dto);
        }

        return resultList;
    }

    public async Task<List<PersonelDto>> GetOrderedBySalaryAsync(bool descending)
    {
        List<Personel> sortedPersonnel = await repo.GetOrderedBySalaryAsync(descending);

        List<PersonelDto> resultList = new List<PersonelDto>();
        foreach (Personel personnel in sortedPersonnel)
        {
            PersonelDto dto = MapToDto(personnel);
            resultList.Add(dto);
        }

        return resultList;
    }

    public async Task<List<PersonelDto>> SearchAsync(string keyword)
    {
        List<Personel> found = await repo.SearchAsync(keyword);

        List<PersonelDto> resultList = new List<PersonelDto>();
        foreach (Personel personnel in found)
        {
            PersonelDto dto = MapToDto(personnel);
            resultList.Add(dto);
        }

        return resultList;
    }

    public async Task<PersonelDto?> GetByIdAsync(int id)
    {
        Personel found = await repo.GetByIdAsync(id);

        if (found == null)
        {
            return null;
        }

        PersonelDto dto = MapToDto(found);
        return dto;
    }

    public async Task<PersonelDto?> GetByEmailAsync(string email)
    {
        Personel found = await repo.GetByEmailAsync(email);

        if (found == null)
        {
            return null;
        }

        PersonelDto dto = MapToDto(found);
        return dto;
    }

    public async Task<PersonelDto> AddAsync(PersonelCreateDto dto)
    {
        Personel newPersonnel = new Personel();
        newPersonnel.Ad = dto.Ad;
        newPersonnel.Soyad = dto.Soyad;
        newPersonnel.Departman = dto.Departman;
        newPersonnel.Unvan = dto.Unvan;
        newPersonnel.Maas = dto.Maas;
        newPersonnel.IseBaslamaTarihi = DateTime.SpecifyKind(dto.IseBaslamaTarihi, DateTimeKind.Utc);
        newPersonnel.Email = dto.Email;
        newPersonnel.Telefon = dto.Telefon;
        newPersonnel.Adres = dto.Adres;
        newPersonnel.Iban = dto.Iban;
        newPersonnel.DogumTarihi = ToUtc(dto.DogumTarihi);

        await repo.AddAsync(newPersonnel);
        await SaveExperiencesAsync(newPersonnel, dto.Experiences);

        User existingUser = await userRepo.GetByEmailAsync(newPersonnel.Email);
        if (existingUser == null)
        {
            Calisan account = new Calisan();
            account.Email = newPersonnel.Email;
            account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(BuildDefaultPassword(newPersonnel.Ad));
            account.Rol = "Calisan";
            account.PersonelId = newPersonnel.Id;

            await userRepo.AddAsync(account);
        }

        PersonelDto result = MapToDto(newPersonnel);
        return result;
    }

    private string BuildDefaultPassword(string ad)
    {
        string trimmed = ad.Trim();
        string capitalized = char.ToUpper(trimmed[0]) + trimmed.Substring(1).ToLower();
        return capitalized + "123!";
    }

    public async Task<PersonelDto?> UpdateAsync(int id, PersonelUpdateDto dto)
    {
        Personel personnel = await repo.GetByIdAsync(id);

        if (personnel == null)
        {
            return null;
        }

        personnel.Ad = dto.Ad;
        personnel.Soyad = dto.Soyad;
        personnel.Departman = dto.Departman;
        personnel.Unvan = dto.Unvan;
        personnel.Maas = dto.Maas;
        personnel.IseBaslamaTarihi = DateTime.SpecifyKind(dto.IseBaslamaTarihi, DateTimeKind.Utc);
        personnel.Email = dto.Email;
        personnel.Telefon = dto.Telefon;
        personnel.Adres = dto.Adres;
        personnel.Iban = dto.Iban;
        personnel.DogumTarihi = ToUtc(dto.DogumTarihi);

        await repo.UpdateAsync(personnel);
        await SaveExperiencesAsync(personnel, dto.Experiences);

        PersonelDto result = MapToDto(personnel);
        return result;
    }

    // Deneyim listesi personel kaydiyla birlikte geliyor.
    // Bos/eksik satirlar atlanir, kalanlar personelin guncel listesi olarak yazilir.
    private async Task SaveExperiencesAsync(Personel personnel, List<ExperienceDto> incoming)
    {
        List<Experience> experienceList = new List<Experience>();

        foreach (ExperienceDto item in incoming)
        {
            if (string.IsNullOrWhiteSpace(item.Company) && string.IsNullOrWhiteSpace(item.Role))
            {
                continue;
            }

            Experience experience = new Experience();
            experience.PersonelId = personnel.Id;
            experience.Company = item.Company ?? string.Empty;
            experience.Role = item.Role ?? string.Empty;
            experience.Duration = item.Duration ?? string.Empty;

            experienceList.Add(experience);
        }

        await experienceRepo.ReplaceForPersonnelAsync(personnel.Id, experienceList);
        personnel.Experiences = experienceList;
    }

    public async Task<bool> UpdateOwnProfileAsync(int id, CalisanProfilUpdateDto dto)
    {
        Personel personnel = await repo.GetByIdAsync(id);

        if (personnel == null)
        {
            return false;
        }

        personnel.Email = dto.Email;
        personnel.Telefon = dto.Telefon;
        personnel.Adres = dto.Adres;
        personnel.Iban = dto.Iban;

        await repo.UpdateAsync(personnel);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        Personel personnel = await repo.GetByIdAsync(id);

        if (personnel == null)
        {
            return false;
        }

        await repo.DeleteAsync(personnel);
        return true;
    }

    private PersonelDto MapToDto(Personel personnel)
    {
        PersonelDto dto = new PersonelDto();
        dto.Id = personnel.Id;
        dto.Ad = personnel.Ad;
        dto.Soyad = personnel.Soyad;
        dto.Departman = personnel.Departman;
        dto.Unvan = personnel.Unvan;
        dto.Maas = personnel.Maas;
        dto.IseBaslamaTarihi = personnel.IseBaslamaTarihi;
        dto.Email = personnel.Email;
        dto.Telefon = personnel.Telefon;
        dto.Adres = personnel.Adres;
        dto.Iban = personnel.Iban;
        dto.DogumTarihi = personnel.DogumTarihi;

        foreach (Experience experience in personnel.Experiences)
        {
            ExperienceDto experienceDto = new ExperienceDto();
            experienceDto.Company = experience.Company;
            experienceDto.Role = experience.Role;
            experienceDto.Duration = experience.Duration;

            dto.Experiences.Add(experienceDto);
        }

        return dto;
    }

    private DateTime? ToUtc(DateTime? value)
    {
        if (value == null)
        {
            return null;
        }

        return DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
    }
}
