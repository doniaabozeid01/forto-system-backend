using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.Abstractions.Services.Clients;
using Forto.Application.DTOs.Bookings.ClientBooking;
using Forto.Application.DTOs.Cars;
using Forto.Application.DTOs.Clients;
using Forto.Domain.Entities.Billings;
using Forto.Domain.Entities.Bookings;
using Forto.Domain.Entities.Clients;
using Forto.Domain.Enum;
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

        private const int PremiumThresholdInvoices = 5;
        private const int PremiumWindowMonths = 6;

        public ClientService(IUnitOfWork uow) => _uow = uow;

        /// <summary>عدد الفواتير المدفوعة من حجوزات (خدمات فقط) في آخر 6 أشهر للعميل.</summary>
        private async Task<int> GetPaidServiceInvoicesCountLast6MonthsAsync(int clientId)
        {
            var from = DateTime.UtcNow.AddMonths(-PremiumWindowMonths);
            var invRepo = _uow.Repository<Invoice>();
            var list = await invRepo.FindAsync(inv =>
                inv.ClientId == clientId &&
                inv.Status == InvoiceStatus.Paid &&
                inv.BookingId != null &&
                inv.PaidAt.HasValue &&
                inv.PaidAt.Value >= from);
            return list.Count;
        }

        private async Task<Dictionary<int, int>> GetPaidServiceInvoicesCountLast6MonthsBatchAsync(IReadOnlyList<int> clientIds)
        {
            if (clientIds == null || clientIds.Count == 0)
                return new Dictionary<int, int>();

            var from = DateTime.UtcNow.AddMonths(-PremiumWindowMonths);
            var invRepo = _uow.Repository<Invoice>();
            var list = await invRepo.FindAsync(inv =>
                inv.ClientId != null &&
                clientIds.Contains(inv.ClientId.Value) &&
                inv.Status == InvoiceStatus.Paid &&
                inv.BookingId != null &&
                inv.PaidAt.HasValue &&
                inv.PaidAt.Value >= from);

            return list
                .GroupBy(inv => inv.ClientId!.Value)
                .ToDictionary(g => g.Key, g => g.Count());
        }

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

            var response = Map(client);
            response.PaidServiceInvoicesCountLast6Months = 0;
            response.IsPremiumCustomer = false;
            return response;
        }

        public async Task<ClientResponse?> GetByIdAsync(int id)
        {
            var client = await _uow.Repository<Client>().GetByIdAsync(id);
            if (client == null) return null;
            var response = Map(client);
            var count = await GetPaidServiceInvoicesCountLast6MonthsAsync(client.Id);
            response.PaidServiceInvoicesCountLast6Months = count;
            response.IsPremiumCustomer = count >= PremiumThresholdInvoices;
            return response;
        }

        //public async Task<IReadOnlyList<ClientResponse>> GetAllAsync()
        //{
        //    var list = await _uow.Repository<Client>().GetAllAsync();
        //    return list.Select(Map).ToList();
        //}





        public async Task<IReadOnlyList<ClientResponse>> GetAllAsync()
        {
            var clientRepo = _uow.Repository<Client>();
            var carRepo = _uow.Repository<Car>();

            // 1) get all clients (ما عدا الـ soft-deleted)
            var clients = await clientRepo.FindAsync(c => !c.IsDeleted);
            if (clients.Count == 0)
                return new List<ClientResponse>();

            // 2) get all cars + premium counts
            var clientIds = clients.Select(c => c.Id).ToList();
            var cars = await carRepo.FindAsync(c => clientIds.Contains(c.ClientId));
            var premiumCounts = await GetPaidServiceInvoicesCountLast6MonthsBatchAsync(clientIds);

            // 3) group cars by client
            var carsByClient = cars
                .GroupBy(c => c.ClientId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 4) map response
            return clients.Select(c =>
            {
                carsByClient.TryGetValue(c.Id, out var clientCars);
                premiumCounts.TryGetValue(c.Id, out var count);

                var response = Map(c);
                response.Cars = (clientCars ?? new List<Car>())
                    .Select(MapClientCar)
                    .ToList();
                response.PaidServiceInvoicesCountLast6Months = count;
                response.IsPremiumCustomer = count >= PremiumThresholdInvoices;

                return response;
            }).ToList();
        }


        private static ClientCarResponse MapClientCar(Car c) => new ClientCarResponse
        {
            Id = c.Id,
            PlateNumber = c.PlateNumber,
            BodyType = c.BodyType,
            IsDefault = c.IsDefault,
            Brand = c.Brand,
            Color = c.Color,
            Model = c.Model,
            Year = c.Year
            // زوّدي أي حقول موجودة عندك في ClientCarResponse
        };











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

            var response = Map(client);
            var count = await GetPaidServiceInvoicesCountLast6MonthsAsync(client.Id);
            response.PaidServiceInvoicesCountLast6Months = count;
            response.IsPremiumCustomer = count >= PremiumThresholdInvoices;
            return response;
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

        //public async Task<ClientLookupResponse?> LookupByPhoneAsync(string phone)
        //{
        //    var normalized = NormalizePhone(phone);
        //    if (string.IsNullOrWhiteSpace(normalized)) return null;

        //    var clientRepo = _uow.Repository<Client>();
        //    var carRepo = _uow.Repository<Car>();

        //    var clients = await clientRepo.FindAsync(c => c.PhoneNumber == normalized);
        //    var client = clients.FirstOrDefault();
        //    if (client == null) return null;

        //    var cars = await carRepo.FindAsync(c => c.ClientId == client.Id);

        //    var carDtos = cars.Select(MapCar).ToList();
        //    var defaultCarId = carDtos.FirstOrDefault(x => x.IsDefault)?.Id;

        //    return new ClientLookupResponse
        //    {
        //        Id = client.Id,
        //        FullName = client.FullName,
        //        PhoneNumber = client.PhoneNumber,
        //        Email = client.Email,
        //        IsActive = client.IsActive,
        //        DefaultCarId = defaultCarId,
        //        Cars = carDtos
        //    };
        //}










        // مع “حد أقصى” للنتائج عشان الأداء (مثلاً 10)
        public async Task<IReadOnlyList<ClientLookupResponse>> SearchByPhoneAsync(string phonePrefix, int take = 10)
        {
            var prefix = NormalizePhone(phonePrefix);
            if (string.IsNullOrWhiteSpace(prefix)) return new List<ClientLookupResponse>();

            var clientRepo = _uow.Repository<Client>();
            var carRepo = _uow.Repository<Car>();

            // 1) هات العملاء اللي رقمهم يبدأ بالprefix
            var clients = await clientRepo.FindAsync(c => c.PhoneNumber.StartsWith(prefix));

            // (اختياري) خدي أول N بس
            clients = clients
                .OrderBy(c => c.PhoneNumber)
                .Take(take)
                .ToList();

            if (clients.Count == 0) return new List<ClientLookupResponse>();

            // 2) هات كل العربيات + حالة العميل المميز
            var clientIds = clients.Select(c => c.Id).ToList();
            var cars = await carRepo.FindAsync(c => clientIds.Contains(c.ClientId));
            var premiumCounts = await GetPaidServiceInvoicesCountLast6MonthsBatchAsync(clientIds);

            var carsByClient = cars.GroupBy(c => c.ClientId).ToDictionary(g => g.Key, g => g.ToList());

            // 3) ابنِ response list
            var result = new List<ClientLookupResponse>();

            foreach (var client in clients)
            {
                carsByClient.TryGetValue(client.Id, out var clientCars);
                clientCars ??= new List<Car>();
                premiumCounts.TryGetValue(client.Id, out var count);

                var carDtos = clientCars.Select(MapCar).ToList();
                var defaultCarId = carDtos.FirstOrDefault(x => x.IsDefault)?.Id;

                result.Add(new ClientLookupResponse
                {
                    Id = client.Id,
                    FullName = client.FullName,
                    PhoneNumber = client.PhoneNumber,
                    Email = client.Email,
                    IsActive = client.IsActive,
                    DefaultCarId = defaultCarId,
                    Cars = carDtos,
                    IsPremiumCustomer = count >= PremiumThresholdInvoices
                });
            }

            return result;
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


























        public async Task<ClientBookingsByStatusResponse> GetClientBookingsByPhoneAsync(string phone)
        {
            var normalized = NormalizePhone(phone);
            if (string.IsNullOrWhiteSpace(normalized))
                throw new BusinessException("Invalid phone", 400);

            var clientRepo = _uow.Repository<Client>();
            var bookingRepo = _uow.Repository<Booking>();
            var carRepo = _uow.Repository<Car>();
            var itemRepo = _uow.Repository<BookingItem>();

            var client = (await clientRepo.FindAsync(c => c.PhoneNumber == normalized)).FirstOrDefault();
            if (client == null)
                throw new BusinessException("Client not found", 404);

            // bookings for client
            var bookings = await bookingRepo.FindAsync(b => b.ClientId == client.Id);

            if (bookings.Count == 0)
                return new ClientBookingsByStatusResponse();

            // cars for plate numbers
            var carIds = bookings.Select(b => b.CarId).Distinct().ToList();
            var cars = await carRepo.FindAsync(c => carIds.Contains(c.Id));
            var carMap = cars.ToDictionary(c => c.Id, c => c);

            // items count per booking (optional)
            var bookingIds = bookings.Select(b => b.Id).ToList();
            var items = await itemRepo.FindAsync(i => bookingIds.Contains(i.BookingId));
            var itemsCountMap = items.GroupBy(i => i.BookingId).ToDictionary(g => g.Key, g => g.Count());

            BookingListItemDto MapBooking(Booking b)
            {
                carMap.TryGetValue(b.CarId, out var car);
                itemsCountMap.TryGetValue(b.Id, out var cnt);

                return new BookingListItemDto
                {
                    BookingId = b.Id,
                    ScheduledStart = b.ScheduledStart,
                    TotalPrice = b.TotalPrice,
                    EstimatedDurationMinutes = b.EstimatedDurationMinutes,
                    Status = b.Status,
                    CarId = b.CarId,
                    PlateNumber = car?.PlateNumber ?? "",
                    ServicesCount = cnt
                };
            }

            // group
            var resp = new ClientBookingsByStatusResponse();

            foreach (var b in bookings.OrderByDescending(x => x.ScheduledStart))
            {
                var dto = MapBooking(b);

                switch (b.Status)
                {
                    case BookingStatus.Pending:
                        resp.Pending.Add(dto);
                        break;
                    case BookingStatus.InProgress:
                        resp.InProgress.Add(dto);
                        break;
                    case BookingStatus.Completed:
                        resp.Completed.Add(dto);
                        break;
                    case BookingStatus.Cancelled:
                        resp.Cancelled.Add(dto);
                        break;
                }
            }

            return resp;
        }

    }
}