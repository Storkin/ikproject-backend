using IkProjesi.DTOs;
using IkProjesi.Models;
using IkProjesi.Repositories;

namespace IkProjesi.Services;

public class LeaveService : ILeaveService
{
    private readonly ILeaveRepository leaveRepo;
    private readonly IPersonnelRepository personnelRepo;
    private readonly ILeaveBalanceRepository balanceRepo;
    private readonly IConfiguration config;

    public LeaveService(
        ILeaveRepository leaveRepository,
        IPersonnelRepository personnelRepository,
        ILeaveBalanceRepository balanceRepository,
        IConfiguration configuration)
    {
        leaveRepo = leaveRepository;
        personnelRepo = personnelRepository;
        balanceRepo = balanceRepository;
        config = configuration;
    }

    // İçinde bulunulan yılın izin hakkı kaydını döndürür.
    // Kayıt yoksa, önceki yıldan kalan gün sayısını devrederek yeni yılın kaydını oluşturur.
    // Aradan birden fazla yıl geçmişse her yıl için ayrı kayıt üretilir.
    private async Task<IzinHakki> EnsureCurrentYearBalanceAsync(int personnelId)
    {
        int currentYear = DateTime.UtcNow.Year;

        IzinHakki current = await balanceRepo.GetByPersonnelAndYearAsync(personnelId, currentYear);
        if (current != null)
        {
            return current;
        }

        int defaultEntitlement = int.Parse(config["PersonelAyarlari:VarsayilanIzinHakki"]);
        IzinHakki previous = await balanceRepo.GetLatestAsync(personnelId);

        if (previous == null)
        {
            IzinHakki firstRecord = new IzinHakki();
            firstRecord.PersonelId = personnelId;
            firstRecord.Yil = currentYear;
            firstRecord.HakEdilen = defaultEntitlement;
            firstRecord.Devreden = 0;

            await balanceRepo.AddAsync(firstRecord);
            return firstRecord;
        }

        IzinHakki lastRecord = previous;
        for (int year = previous.Yil + 1; year <= currentYear; year++)
        {
            int carriedOver = (lastRecord.HakEdilen + lastRecord.Devreden) - lastRecord.Kullanilan;
            if (carriedOver < 0)
            {
                carriedOver = 0;
            }

            IzinHakki newRecord = new IzinHakki();
            newRecord.PersonelId = personnelId;
            newRecord.Yil = year;
            newRecord.HakEdilen = defaultEntitlement;
            newRecord.Devreden = carriedOver;

            await balanceRepo.AddAsync(newRecord);
            lastRecord = newRecord;
        }

        return lastRecord;
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

        IzinHakki balance = await EnsureCurrentYearBalanceAsync(personnelId);
        int totalEntitlement = balance.HakEdilen + balance.Devreden;

        IzinOzetDto summary = new IzinOzetDto();
        summary.Yil = balance.Yil;
        summary.HakEdilenGun = balance.HakEdilen;
        summary.DevredenGun = balance.Devreden;
        summary.ToplamHak = totalEntitlement;
        summary.KullanilanGun = balance.Kullanilan;
        summary.KalanGun = totalEntitlement - balance.Kullanilan;
        summary.KullanilanMazeretGun = balance.KullanilanMazeret;
        summary.KullanilanUcretsizGun = balance.KullanilanUcretsiz;
        summary.Gecmis = historyList;

        return summary;
    }

    public async Task<List<IzinHakkiDto>> GetBalanceHistoryAsync(int personnelId)
    {
        await EnsureCurrentYearBalanceAsync(personnelId);
        List<IzinHakki> allBalances = await balanceRepo.GetByPersonnelIdAsync(personnelId);

        List<IzinHakkiDto> resultList = new List<IzinHakkiDto>();
        foreach (IzinHakki balance in allBalances)
        {
            int totalEntitlement = balance.HakEdilen + balance.Devreden;

            IzinHakkiDto dto = new IzinHakkiDto();
            dto.Yil = balance.Yil;
            dto.HakEdilenGun = balance.HakEdilen;
            dto.DevredenGun = balance.Devreden;
            dto.ToplamHak = totalEntitlement;
            dto.KullanilanGun = balance.Kullanilan;
            dto.KalanGun = totalEntitlement - balance.Kullanilan;
            dto.KullanilanMazeretGun = balance.KullanilanMazeret;
            dto.KullanilanUcretsizGun = balance.KullanilanUcretsiz;

            resultList.Add(dto);
        }

        return resultList;
    }

    // Calisan izin talebi acarken yerine bakacak kisiyi bu listeden secer.
    // Sadece kendi departmanindaki diger personeller doner.
    public async Task<List<SubstituteCandidateDto>> GetSubstituteCandidatesAsync(int personnelId)
    {
        Personel personnel = await personnelRepo.GetByIdAsync(personnelId);
        if (personnel == null)
        {
            return new List<SubstituteCandidateDto>();
        }

        List<Personel> sameDepartment = await personnelRepo.GetByDepartmentAsync(personnel.Departman);

        List<SubstituteCandidateDto> resultList = new List<SubstituteCandidateDto>();
        foreach (Personel colleague in sameDepartment)
        {
            if (colleague.Id == personnelId)
            {
                continue;
            }

            SubstituteCandidateDto dto = new SubstituteCandidateDto();
            dto.Id = colleague.Id;
            dto.AdSoyad = colleague.Ad + " " + colleague.Soyad;
            dto.Unvan = colleague.Unvan;

            resultList.Add(dto);
        }

        return resultList;
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

        DateTime requestStart = DateTime.SpecifyKind(dto.BaslangicTarihi, DateTimeKind.Utc);
        DateTime requestEnd = DateTime.SpecifyKind(dto.BitisTarihi, DateTimeKind.Utc);

        IzinTalep? overlapping = await leaveRepo.GetOverlappingAsync(personnelId, requestStart, requestEnd);
        if (overlapping != null)
        {
            return (false, "Bu tarihlerde zaten bir izin talebiniz var (" +
                           overlapping.BaslangicTarihi.ToString("dd.MM.yyyy") + " - " +
                           overlapping.BitisTarihi.ToString("dd.MM.yyyy") + ", " +
                           overlapping.Durum + ").");
        }

        int requestedDays = (dto.BitisTarihi.Date - dto.BaslangicTarihi.Date).Days + 1;

        if (dto.Turu == IzinTuru.Yillik)
        {
            IzinHakki balance = await EnsureCurrentYearBalanceAsync(personnelId);
            int remainingLeave = (balance.HakEdilen + balance.Devreden) - balance.Kullanilan;

            if (requestedDays > remainingLeave)
            {
                return (false, "Yetersiz izin hakkı. Kalan: " + remainingLeave + " gün.");
            }
        }

        if (dto.SubstituteId != null)
        {
            if (dto.SubstituteId == personnelId)
            {
                return (false, "Kendinizi yerinize bakacak kişi olarak seçemezsiniz.");
            }

            Personel substitute = await personnelRepo.GetByIdAsync(dto.SubstituteId.Value);
            if (substitute == null)
            {
                return (false, "Yerine bakacak kişi bulunamadı.");
            }

            if (substitute.Departman != personnel.Departman)
            {
                return (false, "Yerine bakacak kişi aynı departmandan seçilmelidir.");
            }
        }

        IzinTalep newRequest = new IzinTalep();
        newRequest.PersonelId = personnelId;
        newRequest.SubstituteId = dto.SubstituteId;
        newRequest.BaslangicTarihi = requestStart;
        newRequest.BitisTarihi = requestEnd;
        newRequest.GunSayisi = requestedDays;
        newRequest.Turu = dto.Turu;
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

        IzinHakki balance = await EnsureCurrentYearBalanceAsync(request.PersonelId);

        if (request.Turu == IzinTuru.Yillik)
        {
            int remainingLeave = (balance.HakEdilen + balance.Devreden) - balance.Kullanilan;

            if (request.GunSayisi > remainingLeave)
            {
                return (false, "Personelin izin hakkı yetmiyor. Kalan: " + remainingLeave + " gün.");
            }

            balance.Kullanilan = balance.Kullanilan + request.GunSayisi;
        }
        else if (request.Turu == IzinTuru.Ucretsiz)
        {
            balance.KullanilanUcretsiz = balance.KullanilanUcretsiz + request.GunSayisi;
        }
        else
        {
            balance.KullanilanMazeret = balance.KullanilanMazeret + request.GunSayisi;
        }

        request.Durum = IzinDurum.Onaylandi;

        await balanceRepo.UpdateAsync(balance);
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

        string? substituteFullName = null;
        if (request.Substitute != null)
        {
            substituteFullName = request.Substitute.Ad + " " + request.Substitute.Soyad;
        }

        IzinTalepDto dto = new IzinTalepDto();
        dto.Id = request.Id;
        dto.PersonelId = request.PersonelId;
        dto.PersonelAdSoyad = personnelFullName;
        dto.SubstituteId = request.SubstituteId;
        dto.SubstituteAdSoyad = substituteFullName;
        dto.BaslangicTarihi = request.BaslangicTarihi;
        dto.BitisTarihi = request.BitisTarihi;
        dto.GunSayisi = request.GunSayisi;
        dto.Turu = request.Turu.ToString();
        dto.Durum = request.Durum.ToString();
        dto.TalepTarihi = request.TalepTarihi;
        dto.Aciklama = request.Aciklama;
        return dto;
    }
}
