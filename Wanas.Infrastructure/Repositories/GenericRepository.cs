using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Wanas.Domain.Repositories;
using Wanas.Infrastructure.Persistence;

namespace Wanas.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AppDBContext _context;
        protected readonly DbSet<T> _dbSet;
        public GenericRepository(AppDBContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.Where(predicate).ToListAsync();

        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

        public void Update(T entity) => _dbSet.Update(entity);

        public void Remove(T entity) => _dbSet.Remove(entity);

        public async Task<T?> GetFirstOrDefaultAsync(
            Expression<Func<T, bool>> filter,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string includeProperties = "")
        {
            IQueryable<T> query = _dbSet;
            if (filter != null)
                query = query.Where(filter);

            foreach (var includeProperty in includeProperties.Split(
                new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
                return await orderBy(query).FirstOrDefaultAsync();

            return await query.FirstOrDefaultAsync();
        }
    }
}
