using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.Shifts;
using Forto.Domain.Entities.Employees;

namespace Forto.Application.Abstractions.Services.Shift
{
    public class ShiftService : IShiftService
    {
        private readonly IUnitOfWork _uow;

        public ShiftService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<ShiftResponse> CreateAsync(CreateShiftRequest request)
        {
            // validation business بسيط (اختياري)
            if (request.EndTime == request.StartTime)
            {
                throw new BusinessException(
                    message: "StartTime and EndTime cannot be the same.",
                    statusCode: 400,
                    errors: new Dictionary<string, string[]>
                    {
                        ["endTime"] = new[] { "EndTime must be different from StartTime." }
                    }
                );
            }

            var repo = _uow.Repository<Domain.Entities.Employees.Shift>();

            var shift = new Domain.Entities.Employees.Shift
            {
                Name = request.Name.Trim(),
                StartTime = request.StartTime,
                EndTime = request.EndTime
            };

            await repo.AddAsync(shift);
            await _uow.SaveChangesAsync();

            return new ShiftResponse
            {
                Id = shift.Id,
                Name = shift.Name,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime
            };
        }

        public async Task<IReadOnlyList<ShiftResponse>> GetAllAsync()
        {
            var repo = _uow.Repository<Domain.Entities.Employees.Shift>();
            var shifts = await repo.GetAllAsync();

            return shifts.Select(s => new ShiftResponse
            {
                Id = s.Id,
                Name = s.Name,
                StartTime = s.StartTime,
                EndTime = s.EndTime
            }).ToList();
        }

        public async Task<ShiftResponse?> GetByIdAsync(int id)
        {
            var repo = _uow.Repository<Domain.Entities.Employees.Shift>();
            var s = await repo.GetByIdAsync(id);
            if (s == null) return null;

            return new ShiftResponse
            {
                Id = s.Id,
                Name = s.Name,
                StartTime = s.StartTime,
                EndTime = s.EndTime
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var repo = _uow.Repository<Domain.Entities.Employees.Shift>();
            var s = await repo.GetByIdAsync(id);
            if (s == null) return false;

            repo.Delete(s); // soft delete في generic repo
            await _uow.SaveChangesAsync();
            return true;
        }





        public async Task<ShiftResponse?> UpdateAsync(int id, CreateShiftRequest request)
        {
            if (request.EndTime == request.StartTime)
                throw new Exception("Shift time range is invalid");

            var repo = _uow.Repository<Domain.Entities.Employees.Shift>();
            var shift = await repo.GetByIdAsync(id);
            if (shift == null) return null;

            shift.Name = request.Name.Trim();
            shift.StartTime = request.StartTime;
            shift.EndTime = request.EndTime;

            repo.Update(shift);
            await _uow.SaveChangesAsync();

            return new ShiftResponse
            {
                Id = shift.Id,
                Name = shift.Name,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime
            };
        }



    }
}
