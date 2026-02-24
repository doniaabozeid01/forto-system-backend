using Forto.Application.DTOs.Billings;

namespace Forto.Application.Abstractions.Services.Billings.Tips
{
    public interface ITipsService
    {
        Task<TipResponse> CreateAsync(CreateTipRequest request);
        Task<TipResponse?> GetByIdAsync(int id);
        Task<IReadOnlyList<TipResponse>> GetAllAsync(DateOnly? fromDate = null, DateOnly? toDate = null);
        Task<TipResponse?> UpdateAsync(int id, UpdateTipRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
