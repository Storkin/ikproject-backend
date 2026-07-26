using IkProjesi.DTOs;
using IkProjesi.Models;
using IkProjesi.Repositories;

namespace IkProjesi.Services;

public class PersonnelService : IPersonnelService
{
    private readonly IPersonnelRepository repo;
    private readonly IConfiguration config;

    public PersonnelService(IPersonnelRepository repository, IConfiguration configuration)
    {
        repo = repository;
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

    public async Task<List<PersonelDto>> GetByDepartmentAsync(string department)
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
        newPersonnel.Maas = dto.Maas;
        newPersonnel.IseBaslamaTarihi = dto.IseBaslamaTarihi;
        newPersonnel.Email = dto.Email;
        newPersonnel.YillikIzinHakki = int.Parse(config["PersonelAyarlari:VarsayilanIzinHakki"]);

        await repo.AddAsync(newPersonnel);

        PersonelDto result = MapToDto(newPersonnel);
        return result;
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
        personnel.Maas = dto.Maas;
        personnel.Email = dto.Email;

        await repo.UpdateAsync(personnel);

        PersonelDto result = MapToDto(personnel);
        return result;
    }

    public async Task<bool> UpdateEmailAsync(int id, CalisanEmailUpdateDto dto)
    {
        Personel personnel = await repo.GetByIdAsync(id);

        if (personnel == null)
        {
            return false;
        }

        personnel.Email = dto.Email;
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
        dto.Maas = personnel.Maas;
        dto.IseBaslamaTarihi = personnel.IseBaslamaTarihi;
        dto.Email = personnel.Email;
        return dto;
    }
}
