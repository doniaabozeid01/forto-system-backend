using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Forto.Application.Abstractions.Repositories;
using Forto.Domain.Entities;
using Forto.Infrastructure.Data;
using Forto.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace Forto.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FortoDbContext _db;
        private readonly ConcurrentDictionary<Type, object> _repos = new();
        private IDbContextTransaction? _tx;

        public UnitOfWork(FortoDbContext db) => _db = db;

        public IGenericRepository<T> Repository<T>() where T : BaseEntity
        {
            return (IGenericRepository<T>)_repos.GetOrAdd(typeof(T), _ => new GenericRepository<T>(_db));
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => _db.SaveChangesAsync(cancellationToken);

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_tx != null) return;
            _tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_tx == null) return;
            await _db.SaveChangesAsync(cancellationToken);
            await _tx.CommitAsync(cancellationToken);
            await _tx.DisposeAsync();
            _tx = null;
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_tx == null) return;
            await _tx.RollbackAsync(cancellationToken);
            await _tx.DisposeAsync();
            _tx = null;
        }

        public void Dispose()
        {
            _tx?.Dispose();
            _db.Dispose();
        }
    }
}
