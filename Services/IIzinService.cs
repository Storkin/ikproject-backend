using IkProjesi.DTOs;

namespace IkProjesi.Services;

public interface IIzinService
{
    Task<List<IzinTalepDto>> GetAllAsync();
    Task<List<IzinTalepDto>> GetBekleyenlerAsync();
    Task<List<IzinTalepDto>> GetByPersonelIdAsync(int personelId);
    Task<IzinOzetDto?> GetOzetAsync(int personelId);
    Task<(bool basarili, string mesaj)> TalepOlusturAsync(int personelId, IzinTalepOlusturDto dto);
    Task<(bool basarili, string mesaj)> OnaylaAsync(int talepId);
    Task<(bool basarili, string mesaj)> ReddedAsync(int talepId);
}
