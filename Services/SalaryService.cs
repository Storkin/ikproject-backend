using IkProjesi.DTOs;
using IkProjesi.Models;
using IkProjesi.Repositories;

namespace IkProjesi.Services;

public class SalaryService : ISalaryService
{
    private readonly ISalaryRepository salaryRepo;
    private readonly IPersonnelRepository personnelRepo;

    public SalaryService(ISalaryRepository salaryRepository, IPersonnelRepository personnelRepository)
    {
        salaryRepo = salaryRepository;
        personnelRepo = personnelRepository;
    }

    public async Task<List<MaasKaydiDto>> GetByPersonnelIdAsync(int personnelId)
    {
        List<MaasKaydi> found = await salaryRepo.GetByPersonnelIdAsync(personnelId);

        List<MaasKaydiDto> resultList = new List<MaasKaydiDto>();
        foreach (MaasKaydi record in found)
        {
            MaasKaydiDto dto = MapToDto(record);
            resultList.Add(dto);
        }

        return resultList;
    }

    public async Task<(bool success, string message)> AddAsync(MaasKaydiOlusturDto dto)
    {
        Personel personnel = await personnelRepo.GetByIdAsync(dto.PersonelId);
        if (personnel == null)
        {
            return (false, "Personel bulunamadı.");
        }

        if (dto.Tutar <= 0)
        {
            return (false, "Tutar sıfırdan büyük olmalı.");
        }

        MaasKaydi newRecord = new MaasKaydi();
        newRecord.PersonelId = dto.PersonelId;
        newRecord.Tutar = dto.Tutar;
        newRecord.Tur = dto.Tur;
        newRecord.GecerlilikTarihi = DateTime.SpecifyKind(dto.GecerlilikTarihi, DateTimeKind.Utc);
        newRecord.Aciklama = dto.Aciklama;

        await salaryRepo.AddAsync(newRecord);
        return (true, "Maaş kaydı eklendi.");
    }

    public async Task<(bool success, string message)> DeleteAsync(int id)
    {
        MaasKaydi record = await salaryRepo.GetByIdAsync(id);
        if (record == null)
        {
            return (false, "Maaş kaydı bulunamadı.");
        }

        await salaryRepo.DeleteAsync(record);
        return (true, "Maaş kaydı silindi.");
    }

    private MaasKaydiDto MapToDto(MaasKaydi record)
    {
        string personnelFullName = "";
        if (record.Personel != null)
        {
            personnelFullName = record.Personel.Ad + " " + record.Personel.Soyad;
        }

        MaasKaydiDto dto = new MaasKaydiDto();
        dto.Id = record.Id;
        dto.PersonelId = record.PersonelId;
        dto.PersonelAdSoyad = personnelFullName;
        dto.Tutar = record.Tutar;
        dto.Tur = record.Tur.ToString();
        dto.GecerlilikTarihi = record.GecerlilikTarihi;
        dto.Aciklama = record.Aciklama;
        return dto;
    }
}
