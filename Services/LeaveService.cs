using IkProjesi.DTOs;
using IkProjesi.Models;
using IkProjesi.Repositories;

namespace IkProjesi.Services;

public class LeaveService : ILeaveService
{
    private readonly ILeaveRepository leaveRepo;
    private readonly IPersonnelRepository personnelRepo;

    public LeaveService(ILeaveRepository leaveRepository, IPersonnelRepository personnelRepository)
    {
        leaveRepo = leaveRepository;
        personnelRepo = personnelRepository;
    }

    public async Task<List<IzinTalepDto>> GetAllAsync()
    {
        List<IzinTalep> allRequests = await leaveRepo.GetAllAsync();

        List<IzinTalepDto> resultList = new List<IzinTalepDto>();
        foreach (IzinTalep request in allRequests)
        {
            IzinTalepDto dto = MapToDto(request);
            resultList.Add(dto);
        }

        return resultList;
    }

    public async Task<List<IzinTalepDto>> GetPendingAsync()
    {
        List<IzinTalep> pending = await leaveRepo.GetPendingAsync();

        List<IzinTalepDto> resultList = new List<IzinTalepDto>();
        foreach (IzinTalep request in pending)
        {
            IzinTalepDto dto = MapToDto(request);
            resultList.Add(dto);
        }

        return resultList;
    }

    public async Task<List<IzinTalepDto>> GetByPersonnelIdAsync(int personnelId)
    {
        List<IzinTalep> personnelRequests = await leaveRepo.GetByPersonnelIdAsync(personnelId);

        List<IzinTalepDto> resultList = new List<IzinTalepDto>();
        foreach (IzinTalep request in personnelRequests)
        {
            IzinTalepDto dto = MapToDto(request);
            resultList.Add(dto);
        }

        return resultList;
    }

    public async Task<IzinOzetDto?> GetSummaryAsync(int personnelId)
    {
        Personel personnel = await personnelRepo.GetByIdAsync(personnelId);
        if (personnel == null)
        {
            return null;
        }

        List<IzinTalep> allRequests = await leaveRepo.GetByPersonnelIdAsync(personnelId);

        List<IzinTalepDto> historyList = new List<IzinTalepDto>();
        foreach (IzinTalep request in allRequests)
        {
            IzinTalepDto dto = MapToDto(request);
            historyList.Add(dto);
        }

        int remainingLeave = personnel.YillikIzinHakki - personnel.KullanılanIzin;

        IzinOzetDto summary = new IzinOzetDto();
        summary.ToplamHak = personnel.YillikIzinHakki;
        summary.KullanilanGun = personnel.KullanılanIzin;
        summary.KalanGun = remainingLeave;
        summary.Gecmis = historyList;

        return summary;
    }

    public async Task<(bool success, string message)> CreateRequestAsync(int personnelId, IzinTalepOlusturDto dto)
    {
        if (dto.BitisTarihi < dto.BaslangicTarihi)
        {
            return (false, "Bitiş tarihi başlangıç tarihinden önce olamaz.");
        }

        Personel personnel = await personnelRepo.GetByIdAsync(personnelId);
        if (personnel == null)
        {
            return (false, "Personel bulunamadı.");
        }

        int requestedDays = (dto.BitisTarihi.Date - dto.BaslangicTarihi.Date).Days + 1;
        int remainingLeave = personnel.YillikIzinHakki - personnel.KullanılanIzin;

        if (requestedDays > remainingLeave)
        {
            return (false, "Yetersiz izin hakkı. Kalan: " + remainingLeave + " gün.");
        }

        IzinTalep newRequest = new IzinTalep();
        newRequest.PersonelId = personnelId;
        newRequest.BaslangicTarihi = dto.BaslangicTarihi;
        newRequest.BitisTarihi = dto.BitisTarihi;
        newRequest.GunSayisi = requestedDays;
        newRequest.Aciklama = dto.Aciklama;

        await leaveRepo.AddAsync(newRequest);
        return (true, "İzin talebi oluşturuldu.");
    }

    public async Task<(bool success, string message)> ApproveAsync(int requestId)
    {
        IzinTalep request = await leaveRepo.GetByIdAsync(requestId);
        if (request == null)
        {
            return (false, "Talep bulunamadı.");
        }

        if (request.Durum != IzinDurum.Beklemede)
        {
            return (false, "Bu talep zaten işleme alınmış.");
        }

        Personel personnel = request.Personel;
        int remainingLeave = personnel.YillikIzinHakki - personnel.KullanılanIzin;

        if (request.GunSayisi > remainingLeave)
        {
            return (false, "Personelin izin hakkı yetmiyor. Kalan: " + remainingLeave + " gün.");
        }

        personnel.KullanılanIzin = personnel.KullanılanIzin + request.GunSayisi;
        request.Durum = IzinDurum.Onaylandi;

        await personnelRepo.UpdateAsync(personnel);
        await leaveRepo.UpdateAsync(request);
        return (true, "İzin onaylandı.");
    }

    public async Task<(bool success, string message)> RejectAsync(int requestId)
    {
        IzinTalep request = await leaveRepo.GetByIdAsync(requestId);
        if (request == null)
        {
            return (false, "Talep bulunamadı.");
        }

        if (request.Durum != IzinDurum.Beklemede)
        {
            return (false, "Bu talep zaten işleme alınmış.");
        }

        request.Durum = IzinDurum.Reddedildi;
        await leaveRepo.UpdateAsync(request);
        return (true, "İzin reddedildi.");
    }

    private IzinTalepDto MapToDto(IzinTalep request)
    {
        string personnelFullName = "";
        if (request.Personel != null)
        {
            personnelFullName = request.Personel.Ad + " " + request.Personel.Soyad;
        }

        IzinTalepDto dto = new IzinTalepDto();
        dto.Id = request.Id;
        dto.PersonelId = request.PersonelId;
        dto.PersonelAdSoyad = personnelFullName;
        dto.BaslangicTarihi = request.BaslangicTarihi;
        dto.BitisTarihi = request.BitisTarihi;
        dto.GunSayisi = request.GunSayisi;
        dto.Durum = request.Durum.ToString();
        dto.TalepTarihi = request.TalepTarihi;
        dto.Aciklama = request.Aciklama;
        return dto;
    }
}
