using API_BANKING_PAYMENT.Models;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace API_BANKING_PAYMENT.Respositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly BankDbContext _context;

        public Repository(BankDbContext context)
        {
            _context = context;
        }

        public async Task<T> GetById(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<bool> Add(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            return await Save();
        }

        public async Task<bool> Update(T entity)
        {
            _context.Set<T>().Update(entity);
            return await Save();
        }

        public async Task<bool> Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
            return await Save();
        }

        private async Task<bool> Save()
        {
            var saved = await _context.SaveChangesAsync();
            return saved > 0;
        }
    }
}
