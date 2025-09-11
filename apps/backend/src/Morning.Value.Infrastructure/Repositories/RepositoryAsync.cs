using Microsoft.EntityFrameworkCore;
using Morning.Value.Domain.Common.Interfaces;
using Morning.Value.Infrastructure.Persistences.Contexts;

namespace Morning.Value.Infrastructure.Repositories
{
    public class RepositoryAsync<T> : IRepositoryAsync<T> where T : class
    {
        private readonly AppDbContext _dbContext;
        protected readonly DbSet<T> _entities;

        public RepositoryAsync(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _entities = _dbContext.Set<T>();
        }
        public async Task<int> CountAsync()
        {
            return await _entities.CountAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            T entity = await this.GetAsync(id);
            if (entity is not null)
            {
                _entities.Remove(entity);
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _entities.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllWithPaginationAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            return await _entities
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<T> GetAsync(Guid id)
        {
            return await _entities.FindAsync(id) ?? throw new KeyNotFoundException($"Entity with id {id} not found.");
        }

        public async Task<bool> InsertAsync(T entity)
        {
            await _entities.AddAsync(entity);
            return true;
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            _entities.Update(entity);
            return await Task.FromResult(true);
        }
    }
}
