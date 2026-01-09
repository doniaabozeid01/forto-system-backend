using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Forto.Application.Abstractions.Repositories;
using Forto.Domain.Entities;
using Forto.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Forto.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly FortoDbContext _db;
        private readonly DbSet<T> _set;

        public GenericRepository(FortoDbContext db)
        {
            _db = db;
            _set = db.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id)
            => await _set.FirstOrDefaultAsync(x => x.Id == id);

        public async Task<IReadOnlyList<T>> GetAllAsync()
            => await _set.AsNoTracking().ToListAsync();

        public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate)
            => await _set.AsNoTracking().Where(predicate).ToListAsync();

        public async Task AddAsync(T entity)
            => await _set.AddAsync(entity);

        public async Task AddRangeAsync(IEnumerable<T> entities)
            => await _set.AddRangeAsync(entities);

        public void Update(T entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _set.Update(entity);
        }

        public void Delete(T entity)
        {
            // Soft delete
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            _set.Update(entity);

            // لو عايزة hard delete بدل soft:
            // _set.Remove(entity);
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
            => await _set.AnyAsync(predicate);

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
            => await _set.CountAsync(predicate);
    }
}
