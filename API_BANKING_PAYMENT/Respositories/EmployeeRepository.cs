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

        public async Task<(IEnumerable<Employee> Employees, int TotalCount)> GetPaginatedAsync(
            long clientId,  // Add clientId parameter
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? sortBy = null,
            bool sortDescending = false)
        {
            // Start with client filter
            var query = _context.Employees.Where(e => e.ClientId == clientId);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(e =>
                    e.FullName.Contains(searchTerm) ||
                    e.Email.Contains(searchTerm) ||
                    e.PhoneNumber.Contains(searchTerm) ||
                    e.BankName.Contains(searchTerm) ||
                    e.Ifsccode.Contains(searchTerm));
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                query = sortBy.ToLower() switch
                {
                    "email" => sortDescending ? query.OrderByDescending(e => e.Email) : query.OrderBy(e => e.Email),
                    "fullname" => sortDescending ? query.OrderByDescending(e => e.FullName) : query.OrderBy(e => e.FullName),
                    "createdat" => sortDescending ? query.OrderByDescending(e => e.CreatedAt) : query.OrderBy(e => e.CreatedAt),
                    "salaryamount" => sortDescending ? query.OrderByDescending(e => e.SalaryAmount) : query.OrderBy(e => e.SalaryAmount),
                    "accountnumber" => sortDescending ? query.OrderByDescending(e => e.AccountNumber) : query.OrderBy(e => e.AccountNumber),
                    "bankname" => sortDescending ? query.OrderByDescending(e => e.BankName) : query.OrderBy(e => e.BankName),
                    _ => sortDescending ? query.OrderByDescending(e => e.EmployeeId) : query.OrderBy(e => e.EmployeeId)
                };
            }
            else
            {
                query = query.OrderBy(e => e.EmployeeId);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var employees = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (employees, totalCount);
        }
    }

}
