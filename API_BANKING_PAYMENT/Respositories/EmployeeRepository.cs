using API_BANKING_PAYMENT.Models;
using API_BANKING_PAYMENT.Models.Entities;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace API_BANKING_PAYMENT.Respositories
{
    public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
    {
        private readonly BankDbContext _context;

        public EmployeeRepository(BankDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == username);
        }
        public async Task<bool> EmployeeExistsAsync(long clientId, string email, long accountNumber)
        {
            return await _context.Employees
                .AnyAsync(e => e.ClientId == clientId &&
                              (e.Email == email || e.AccountNumber == accountNumber));
        }

        public async Task<bool> AddRangeAsync(IEnumerable<Employee> employees)
        {
            await _context.Employees.AddRangeAsync(employees);
            return await _context.SaveChangesAsync() > 0;
        }
    }

}
