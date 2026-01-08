using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.Cars;
using Forto.Domain.Entities.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.Abstractions.Services.Cars
{
    public class CarService : ICarService
    {
        private readonly IUnitOfWork _uow;

        public CarService(IUnitOfWork uow) => _uow = uow;

        public async Task<CarResponse> AddToClientAsync(int clientId, CreateCarRequest request)
        {
            var clientRepo = _uow.Repository<Client>();
            var carRepo = _uow.Repository<Car>();

            var client = await clientRepo.GetByIdAsync(clientId);
            if (client == null)
                throw new BusinessException("Client not found", 404);

            var plate = NormalizePlate(request.PlateNumber);

            // uniqueness per client
            var exists = await carRepo.AnyAsync(x => x.ClientId == clientId && x.PlateNumber == plate);
            if (exists)
                throw new BusinessException("Car already exists for this client", 409,
                    new Dictionary<string, string[]> { ["plateNumber"] = new[] { "Already added." } });

            // لو دي default، لازم نشيل default من باقي عربياته
            if (request.IsDefault)
            {
                var existingCars = await carRepo.FindAsync(x => x.ClientId == clientId);
                foreach (var c in existingCars.Where(x => x.IsDefault))
                {
                    c.IsDefault = false;
                    carRepo.Update(c);
                }
            }

            var car = new Car
            {
                ClientId = clientId,
                PlateNumber = plate,
                BodyType = request.BodyType,
                Brand = request.Brand?.Trim(),
                Model = request.Model?.Trim(),
                Color = request.Color?.Trim(),
                Year = request.Year,
                IsDefault = request.IsDefault
            };

            await carRepo.AddAsync(car);
            await _uow.SaveChangesAsync();

            return Map(car);
        }

        public async Task<CarResponse?> UpdateAsync(int carId, UpdateCarRequest request)
        {
            var carRepo = _uow.Repository<Car>();
            var car = await carRepo.GetByIdAsync(carId);
            if (car == null) return null;

            var plate = NormalizePlate(request.PlateNumber);

            // uniqueness per client (مع استثناء نفس العربية)
            var exists = await carRepo.AnyAsync(x =>
                x.ClientId == car.ClientId &&
                x.PlateNumber == plate &&
                x.Id != carId);

            if (exists)
                throw new BusinessException("Car already exists for this client", 409,
                    new Dictionary<string, string[]> { ["plateNumber"] = new[] { "Already added." } });

            // default logic
            if (request.IsDefault && !car.IsDefault)
            {
                var siblings = await carRepo.FindAsync(x => x.ClientId == car.ClientId);
                foreach (var s in siblings.Where(x => x.IsDefault))
                {
                    s.IsDefault = false;
                    carRepo.Update(s);
                }
            }

            car.PlateNumber = plate;
            car.BodyType = request.BodyType;
            car.Brand = request.Brand?.Trim();
            car.Model = request.Model?.Trim();
            car.Color = request.Color?.Trim();
            car.Year = request.Year;
            car.IsDefault = request.IsDefault;

            carRepo.Update(car);
            await _uow.SaveChangesAsync();

            return Map(car);
        }

        public async Task<bool> DeleteAsync(int carId)
        {
            var carRepo = _uow.Repository<Car>();
            var car = await carRepo.GetByIdAsync(carId);
            if (car == null) return false;

            carRepo.Delete(car); // soft delete
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<CarResponse?> SetDefaultAsync(int clientId, int carId)
        {
            var carRepo = _uow.Repository<Car>();
            var cars = await carRepo.FindAsync(x => x.ClientId == clientId);

            var target = cars.FirstOrDefault(x => x.Id == carId);
            if (target == null) return null;

            foreach (var c in cars)
            {
                c.IsDefault = (c.Id == carId);
                carRepo.Update(c);
            }

            await _uow.SaveChangesAsync();
            return Map(target);
        }

        private static CarResponse Map(Car x) => new()
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

        private static string NormalizePlate(string? plate)
        {
            // trims + uppercase + remove extra spaces
            var p = (plate ?? "").Trim().ToUpperInvariant();
            while (p.Contains("  ")) p = p.Replace("  ", " ");
            return p;
        }
    }

}
