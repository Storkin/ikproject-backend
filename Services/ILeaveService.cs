using IkProjesi.DTOs;

namespace IkProjesi.Services;

public interface ILeaveService
{
    Task<List<IzinTalepDto>> GetAllAsync();
    Task<List<IzinTalepDto>> GetPendingAsync();
    Task<List<IzinTalepDto>> GetByPersonnelIdAsync(int personnelId);
    Task<IzinOzetDto?> GetSummaryAsync(int personnelId);
    Task<List<IzinHakkiDto>> GetBalanceHistoryAsync(int personnelId);
    Task<(bool success, string message)> CreateRequestAsync(int personnelId, IzinTalepOlusturDto dto);
    Task<(bool success, string message)> ApproveAsync(int requestId);
    Task<(bool success, string message)> RejectAsync(int requestId);
}
