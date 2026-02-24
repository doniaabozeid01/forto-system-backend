using Forto.Api.Common;
using Forto.Application.Abstractions.Repositories;
using Forto.Application.DTOs.Billings;
using TipsEntity = Forto.Domain.Entities.Billings.Tips;

namespace Forto.Application.Abstractions.Services.Billings.Tips
{
    public class TipsService : ITipsService
    {
        private readonly IUnitOfWork _uow;

        public TipsService(IUnitOfWork uow) => _uow = uow;

        public async Task<TipResponse> CreateAsync(CreateTipRequest request)
        {
            if (!DateOnly.TryParse(request.TipsDate, out var tipsDate))
                throw new BusinessException("Invalid TipsDate. Use format like 2026-4-2 (year-month-day).", 400,
                    new Dictionary<string, string[]> { ["tipsDate"] = new[] { "Invalid date format." } });

            if (request.CashierId.HasValue)
            {
                var emp = await _uow.Repository<Forto.Domain.Entities.Employees.Employee>().GetByIdAsync(request.CashierId.Value);
                if (emp == null || !emp.IsActive)
                    throw new BusinessException("Cashier not found or inactive", 404);
            }

            var entity = new TipsEntity
            {
                Amount = request.Amount,
                TipsDate = tipsDate,
                CashierId = request.CashierId
            };

            var repo = _uow.Repository<TipsEntity>();
            await repo.AddAsync(entity);
            await _uow.SaveChangesAsync();

            return Map(entity);
        }

        public async Task<TipResponse?> GetByIdAsync(int id)
        {
            var entity = await _uow.Repository<TipsEntity>().GetByIdAsync(id);
            return entity == null ? null : Map(entity);
        }

        public async Task<TipsListResponse> GetAllAsync(DateOnly? fromDate = null, DateOnly? toDate = null)
        {
            var repo = _uow.Repository<TipsEntity>();
            var list = await repo.FindAsync(x =>
                (!fromDate.HasValue || x.TipsDate >= fromDate.Value) &&
                (!toDate.HasValue || x.TipsDate <= toDate.Value));

            var ordered = list.OrderByDescending(x => x.TipsDate).ThenByDescending(x => x.Id).ToList();
            var items = ordered.Select(Map).ToList();
            var total = items.Sum(x => x.Amount);

            return new TipsListResponse
            {
                Items = items,
                Total = total
            };
        }

        public async Task<TipResponse?> UpdateAsync(int id, UpdateTipRequest request)
        {
            var repo = _uow.Repository<TipsEntity>();
            var entity = await repo.GetByIdAsync(id);
            if (entity == null) return null;

            if (request.CashierId.HasValue)
            {
                var emp = await _uow.Repository<Forto.Domain.Entities.Employees.Employee>().GetByIdAsync(request.CashierId.Value);
                if (emp == null || !emp.IsActive)
                    throw new BusinessException("Cashier not found or inactive", 404);
            }

            if (!DateOnly.TryParse(request.TipsDate, out var tipsDate))
                throw new BusinessException("Invalid TipsDate. Use format like 2026-4-2 (year-month-day).", 400,
                    new Dictionary<string, string[]> { ["tipsDate"] = new[] { "Invalid date format." } });

            entity.Amount = request.Amount;
            entity.TipsDate = tipsDate;
            entity.CashierId = request.CashierId;
            entity.UpdatedAt = DateTime.UtcNow;

            repo.Update(entity);
            await _uow.SaveChangesAsync();

            return Map(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var repo = _uow.Repository<TipsEntity>();
            var entity = await repo.GetByIdAsync(id);
            if (entity == null) return false;

            repo.Delete(entity);
            await _uow.SaveChangesAsync();
            return true;
        }

        private static TipResponse Map(TipsEntity x) => new()
        {
            Id = x.Id,
            Amount = x.Amount,
            TipsDate = x.TipsDate,
            CashierId = x.CashierId,
            CreatedAt = x.CreatedAt
        };
    }
}
