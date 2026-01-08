using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.Abstractions.Services.Clients;
using Forto.Application.DTOs.Cars;
using Forto.Application.DTOs.Clients;
using Forto.Domain.Entities.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.Abstractions.Services.Clients
{
    public class ClientService : IClientService
    {
        private readonly IUnitOfWork _uow;

        public ClientService(IUnitOfWork uow) => _uow = uow;

        public async Task<ClientResponse> CreateAsync(CreateClientRequest request)
        {
            var repo = _uow.Repository<Client>();

            var phone = NormalizePhone(request.PhoneNumber);

            var exists = await repo.AnyAsync(x => x.PhoneNumber == phone);
            if (exists)
                throw new BusinessException("Phone number already exists", 409,
                    new Dictionary<string, string[]> { ["phoneNumber"] = new[] { "Already used." } });

            var client = new Client
            {
                FullName = request.FullName.Trim(),
                PhoneNumber = phone,
                Email = request.Email?.Trim(),
                Notes = request.Notes?.Trim(),
                IsActive = true
            };

            await repo.AddAsync(client);
            await _uow.SaveChangesAsync();

            return Map(client);
        }

        public async Task<ClientResponse?> GetByIdAsync(int id)
        {
            var client = await _uow.Repository<Client>().GetByIdAsync(id);
            return client == null ? null : Map(client);
        }

        public async Task<IReadOnlyList<ClientResponse>> GetAllAsync()
        {
            var list = await _uow.Repository<Client>().GetAllAsync();
            return list.Select(Map).ToList();
        }

        public async Task<ClientResponse?> UpdateAsync(int id, UpdateClientRequest request)
        {
            var repo = _uow.Repository<Client>();
            var client = await repo.GetByIdAsync(id);
            if (client == null) return null;

            var newPhone = NormalizePhone(request.PhoneNumber);

            // لو غير الرقم، اتأكد unique
            if (!string.Equals(client.PhoneNumber, newPhone, StringComparison.OrdinalIgnoreCase))
            {
                var exists = await repo.AnyAsync(x => x.PhoneNumber == newPhone && x.Id != id);
                if (exists)
                    throw new BusinessException("Phone number already exists", 409,
                        new Dictionary<string, string[]> { ["phoneNumber"] = new[] { "Already used." } });
            }

            client.FullName = request.FullName.Trim();
            client.PhoneNumber = newPhone;
            client.Email = request.Email?.Trim();
            client.Notes = request.Notes?.Trim();
            client.IsActive = request.IsActive;

            repo.Update(client);
            await _uow.SaveChangesAsync();

            return Map(client);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var repo = _uow.Repository<Client>();
            var client = await repo.GetByIdAsync(id);
            if (client == null) return false;

            repo.Delete(client); // soft delete
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<ClientLookupResponse?> LookupByPhoneAsync(string phone)
        {
            var normalized = NormalizePhone(phone);
            if (string.IsNullOrWhiteSpace(normalized)) return null;

            var clientRepo = _uow.Repository<Client>();
            var carRepo = _uow.Repository<Car>();

            var clients = await clientRepo.FindAsync(c => c.PhoneNumber == normalized);
            var client = clients.FirstOrDefault();
            if (client == null) return null;

            var cars = await carRepo.FindAsync(c => c.ClientId == client.Id);

            var carDtos = cars.Select(MapCar).ToList();
            var defaultCarId = carDtos.FirstOrDefault(x => x.IsDefault)?.Id;

            return new ClientLookupResponse
            {
                Id = client.Id,
                FullName = client.FullName,
                PhoneNumber = client.PhoneNumber,
                Email = client.Email,
                IsActive = client.IsActive,
                DefaultCarId = defaultCarId,
                Cars = carDtos
            };
        }

        private static ClientResponse Map(Client c) => new()
        {
            Id = c.Id,
            FullName = c.FullName,
            PhoneNumber = c.PhoneNumber,
            Email = c.Email,
            Notes = c.Notes,
            IsActive = c.IsActive
        };

        private static CarResponse MapCar(Car x) => new()
        {
            Id = x.Id,
            ClientId = x.ClientId,
            PlateNumber = x.PlateNumber,
            BodyType = x.BodyType,
            Brand = x.Brand,
            Model = x.Model,
            Color = x.Color,
            Year = x.Year,
            IsDefault = x.IsDefault
        };

        private static string NormalizePhone(string? phone) => (phone ?? "").Trim();

    }
}