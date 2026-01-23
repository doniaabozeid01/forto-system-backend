using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Forto.Domain.Entities;

namespace Forto.Application.Abstractions.Repositories
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> GetAllAsync();

        Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<IReadOnlyList<T>> FindTrackingAsync(Expression<Func<T, bool>> predicate);


        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);

        void Update(T entity);
        void Delete(T entity); // هنا ممكن تخليه soft delete في التنفيذ
        void HardDelete(T entity); // هنا ممكن تخليه soft delete في التنفيذ

        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    }
}
