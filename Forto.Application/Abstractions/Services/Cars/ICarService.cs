using Forto.Application.DTOs.Cars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.Abstractions.Services.Cars
{
    public interface ICarService
    {
        Task<CarResponse> AddToClientAsync(int clientId, CreateCarRequest request);
        Task<CarResponse?> UpdateAsync(int carId, UpdateCarRequest request);
        Task<bool> DeleteAsync(int carId);

        Task<CarResponse?> SetDefaultAsync(int clientId, int carId);
    }
}
