using IkProjesi.Data;
using IkProjesi.Models;
using Microsoft.EntityFrameworkCore;

namespace IkProjesi.Repositories;

public class LeaveRepository : ILeaveRepository
{
    private readonly AppDbContext db;

    public LeaveRepository(AppDbContext context)
    {
        db = context;
    }

    // Onay bekleyen talepler tarihinden bagimsiz olarak en uste gelir;
    // IK'nin islem yapmasi gereken kayitlar listenin dibinde kaybolmasin.
    // Ayni grup icinde en yeni talep once gosterilir.
    public async Task<List<IzinTalep>> GetAllAsync()
    {
        List<IzinTalep> allRequests = await db.IzinTalepler
            .Include(t => t.Personel)
            .Include(t => t.Substitute)
            .OrderBy(t => t.Durum == IzinDurum.Beklemede ? 0 : 1)
            .ThenByDescending(t => t.TalepTarihi)
            .ThenByDescending(t => t.Id)
            .ToListAsync();

        return allRequests;
    }

    public async Task<List<IzinTalep>> GetPendingAsync()
    {
        List<IzinTalep> pendingRequests = await db.IzinTalepler
            .Include(t => t.Personel)
            .Include(t => t.Substitute)
            .Where(t => t.Durum == IzinDurum.Beklemede)
            .OrderByDescending(t => t.TalepTarihi)
            .ThenByDescending(t => t.Id)
            .ToListAsync();

        return pendingRequests;
    }

    public async Task<List<IzinTalep>> GetByPersonnelIdAsync(int personnelId)
    {
        List<IzinTalep> personnelRequests = await db.IzinTalepler
            .Include(t => t.Personel)
            .Include(t => t.Substitute)
            .Where(t => t.PersonelId == personnelId)
            .OrderBy(t => t.Durum == IzinDurum.Beklemede ? 0 : 1)
            .ThenByDescending(t => t.TalepTarihi)
            .ThenByDescending(t => t.Id)
            .ToListAsync();

        return personnelRequests;
    }

    public async Task<IzinTalep> GetByIdAsync(int id)
    {
        IzinTalep found = await db.IzinTalepler
            .Include(t => t.Personel)
            .Include(t => t.Substitute)
            .FirstOrDefaultAsync(t => t.Id == id);

        return found;
    }

    // Ayni personelin tarihleri cakisan baska bir talebi var mi?
    // Reddedilen talepler cakisma sayilmaz.
    public async Task<IzinTalep?> GetOverlappingAsync(int personnelId, DateTime start, DateTime end)
    {
        IzinTalep found = await db.IzinTalepler
            .Where(t => t.PersonelId == personnelId &&
                        t.Durum != IzinDurum.Reddedildi &&
                        t.BaslangicTarihi <= end &&
                        t.BitisTarihi >= start)
            .FirstOrDefaultAsync();

        return found;
    }

    // Yerine bakacak kisi isten ayrildiginda, HENUZ ONAYLANMAMIS taleplerdeki
    // bagi kopariyoruz. Onaylanmis/reddedilmis kayitlar tarihsel kayit oldugu
    // icin oldugu gibi birakilir.
    public async Task ClearSubstituteFromPendingAsync(int substituteId)
    {
        List<IzinTalep> bekleyenler = await db.IzinTalepler
            .Where(t => t.SubstituteId == substituteId && t.Durum == IzinDurum.Beklemede)
            .ToListAsync();

        foreach (IzinTalep request in bekleyenler)
        {
            request.SubstituteId = null;
        }

        if (bekleyenler.Count > 0)
        {
            await db.SaveChangesAsync();
        }
    }

    public async Task AddAsync(IzinTalep request)
    {
        await db.IzinTalepler.AddAsync(request);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(IzinTalep request)
    {
        db.IzinTalepler.Update(request);
        await db.SaveChangesAsync();
    }
}
