using IkProjesi.DTOs;
using IkProjesi.Models;
using IkProjesi.Repositories;

namespace IkProjesi.Services;

public class EquipmentService : IEquipmentService
{
    private readonly IEquipmentRepository equipmentRepo;
    private readonly IPersonnelRepository personnelRepo;

    public EquipmentService(IEquipmentRepository equipmentRepository, IPersonnelRepository personnelRepository)
    {
        equipmentRepo = equipmentRepository;
        personnelRepo = personnelRepository;
    }

    public async Task<List<ZimmetDto>> GetAllAsync()
    {
        List<Zimmet> found = await equipmentRepo.GetAllAsync();

        List<ZimmetDto> resultList = new List<ZimmetDto>();
        foreach (Zimmet equipment in found)
        {
            ZimmetDto dto = MapToDto(equipment);
            resultList.Add(dto);
        }

        return resultList;
    }

    public async Task<List<ZimmetDto>> GetByPersonnelIdAsync(int personnelId)
    {
        List<Zimmet> found = await equipmentRepo.GetByPersonnelIdAsync(personnelId);

        List<ZimmetDto> resultList = new List<ZimmetDto>();
        foreach (Zimmet equipment in found)
        {
            ZimmetDto dto = MapToDto(equipment);
            resultList.Add(dto);
        }

        return resultList;
    }

    public async Task<(bool success, string message)> AssignAsync(ZimmetOlusturDto dto)
    {
        Personel personnel = await personnelRepo.GetByIdAsync(dto.PersonelId);
        if (personnel == null)
        {
            return (false, "Personel bulunamadı.");
        }

        if (string.IsNullOrWhiteSpace(dto.EsyaAdi))
        {
            return (false, "Eşya adı boş olamaz.");
        }

        Zimmet newEquipment = new Zimmet();
        newEquipment.PersonelId = dto.PersonelId;
        newEquipment.EsyaAdi = dto.EsyaAdi;
        newEquipment.SeriNo = dto.SeriNo;
        newEquipment.ZimmetTarihi = DateTime.SpecifyKind(dto.ZimmetTarihi, DateTimeKind.Utc);
        newEquipment.Aciklama = dto.Aciklama;

        await equipmentRepo.AddAsync(newEquipment);
        return (true, "Zimmet kaydı oluşturuldu.");
    }

    public async Task<(bool success, string message)> ReturnAsync(int id)
    {
        Zimmet equipment = await equipmentRepo.GetByIdAsync(id);
        if (equipment == null)
        {
            return (false, "Zimmet kaydı bulunamadı.");
        }

        if (equipment.IadeTarihi != null)
        {
            return (false, "Bu eşya zaten iade edilmiş.");
        }

        equipment.IadeTarihi = DateTime.UtcNow;
        await equipmentRepo.UpdateAsync(equipment);
        return (true, "Eşya iade alındı.");
    }

    public async Task<(bool success, string message)> DeleteAsync(int id)
    {
        Zimmet equipment = await equipmentRepo.GetByIdAsync(id);
        if (equipment == null)
        {
            return (false, "Zimmet kaydı bulunamadı.");
        }

        await equipmentRepo.DeleteAsync(equipment);
        return (true, "Zimmet kaydı silindi.");
    }

    private ZimmetDto MapToDto(Zimmet equipment)
    {
        string personnelFullName = "";
        if (equipment.Personel != null)
        {
            personnelFullName = equipment.Personel.Ad + " " + equipment.Personel.Soyad;
        }

        ZimmetDto dto = new ZimmetDto();
        dto.Id = equipment.Id;
        dto.PersonelId = equipment.PersonelId;
        dto.PersonelAdSoyad = personnelFullName;
        dto.EsyaAdi = equipment.EsyaAdi;
        dto.SeriNo = equipment.SeriNo;
        dto.ZimmetTarihi = equipment.ZimmetTarihi;
        dto.IadeTarihi = equipment.IadeTarihi;
        dto.Aciklama = equipment.Aciklama;
        return dto;
    }
}
