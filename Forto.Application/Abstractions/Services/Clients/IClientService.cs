using Forto.Application.DTOs.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.Abstractions.Services.Clients
{
    public interface IClientService
    {
        Task<ClientResponse> CreateAsync(CreateClientRequest request);
        Task<ClientResponse?> GetByIdAsync(int id);
        Task<IReadOnlyList<ClientResponse>> GetAllAsync();
        Task<ClientResponse?> UpdateAsync(int id, UpdateClientRequest request);
        Task<bool> DeleteAsync(int id);

        Task<ClientLookupResponse?> LookupByPhoneAsync(string phone);
    }
}
