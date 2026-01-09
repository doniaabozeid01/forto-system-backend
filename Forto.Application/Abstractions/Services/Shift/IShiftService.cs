using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Application.DTOs.Shifts;

namespace Forto.Application.Abstractions.Services.Shift
{
    public interface IShiftService
    {
        Task<ShiftResponse> CreateAsync(CreateShiftRequest request);
        Task<IReadOnlyList<ShiftResponse>> GetAllAsync();
        Task<ShiftResponse?> GetByIdAsync(int id);
        Task<ShiftResponse?> UpdateAsync(int id, CreateShiftRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
