using IkProjesi.DTOs;
using IkProjesi.Models;
using IkProjesi.Repositories;

namespace IkProjesi.Services;

public class EducationService : IEducationService
{
    private readonly IEducationRepository educationRepo;
    private readonly IPersonnelRepository personnelRepo;

    public EducationService(IEducationRepository educationRepository, IPersonnelRepository personnelRepository)
    {
        educationRepo = educationRepository;
        personnelRepo = personnelRepository;
    }

    public async Task<List<EgitimDto>> GetAllAsync()
    {
        List<Egitim> found = await educationRepo.GetAllAsync();

        List<EgitimDto> resultList = new List<EgitimDto>();
        foreach (Egitim record in found)
        {
            EgitimDto dto = MapToDto(record);
            resultList.Add(dto);
        }

        return resultList;
    }

    public async Task<List<EgitimDto>> GetByPersonnelIdAsync(int personnelId)
    {
        List<Egitim> found = await educationRepo.GetByPersonnelIdAsync(personnelId);

        List<EgitimDto> resultList = new List<EgitimDto>();
        foreach (Egitim education in found)
        {
            EgitimDto dto = MapToDto(education);
            resultList.Add(dto);
        }

        return resultList;
    }

    public async Task<(bool success, string message)> AddAsync(EgitimOlusturDto dto)
    {
        Personel personnel = await personnelRepo.GetByIdAsync(dto.PersonelId);
        if (personnel == null)
        {
            return (false, "Personel bulunamadı.");
        }

        if (string.IsNullOrWhiteSpace(dto.Ad))
        {
            return (false, "Eğitim adı boş olamaz.");
        }

        Egitim newEducation = new Egitim();
        newEducation.PersonelId = dto.PersonelId;
        newEducation.Ad = dto.Ad;
        newEducation.Kurum = dto.Kurum;
        newEducation.TamamlanmaTarihi = DateTime.SpecifyKind(dto.TamamlanmaTarihi, DateTimeKind.Utc);
        newEducation.GecerlilikTarihi = ToUtc(dto.GecerlilikTarihi);
        newEducation.Aciklama = dto.Aciklama;

        await educationRepo.AddAsync(newEducation);
        return (true, "Eğitim kaydı eklendi.");
    }

    public async Task<(bool success, string message)> UpdateAsync(int id, EgitimGuncelleDto dto)
    {
        Egitim education = await educationRepo.GetByIdAsync(id);
        if (education == null)
        {
            return (false, "Eğitim kaydı bulunamadı.");
        }

        if (string.IsNullOrWhiteSpace(dto.Ad))
        {
            return (false, "Eğitim adı boş olamaz.");
        }

        education.Ad = dto.Ad;
        education.Kurum = dto.Kurum;
        education.TamamlanmaTarihi = DateTime.SpecifyKind(dto.TamamlanmaTarihi, DateTimeKind.Utc);
        education.GecerlilikTarihi = ToUtc(dto.GecerlilikTarihi);
        education.Aciklama = dto.Aciklama;

        await educationRepo.UpdateAsync(education);
        return (true, "Eğitim kaydı güncellendi.");
    }

    public async Task<(bool success, string message)> DeleteAsync(int id)
    {
        Egitim education = await educationRepo.GetByIdAsync(id);
        if (education == null)
        {
            return (false, "Eğitim kaydı bulunamadı.");
        }

        await educationRepo.DeleteAsync(education);
        return (true, "Eğitim kaydı silindi.");
    }

    private EgitimDto MapToDto(Egitim education)
    {
        string personnelFullName = "";
        if (education.Personel != null)
        {
            personnelFullName = education.Personel.Ad + " " + education.Personel.Soyad;
        }

        EgitimDto dto = new EgitimDto();
        dto.Id = education.Id;
        dto.PersonelId = education.PersonelId;
        dto.PersonelAdSoyad = personnelFullName;
        dto.Ad = education.Ad;
        dto.Kurum = education.Kurum;
        dto.TamamlanmaTarihi = education.TamamlanmaTarihi;
        dto.GecerlilikTarihi = education.GecerlilikTarihi;
        dto.Aciklama = education.Aciklama;
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
