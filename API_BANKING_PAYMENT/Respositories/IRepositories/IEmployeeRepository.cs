using API_BANKING_PAYMENT.Models;
using API_BANKING_PAYMENT.Models.Entities;

namespace API_BANKING_PAYMENT.Respositories.IRepositories
{
    public interface IEmployeeRepository : IRepository<Employee>
    {
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByUsernameAsync(string username);
        Task<bool> EmployeeExistsAsync(long clientId, string email, long accountNumber);
        Task<bool> AddRangeAsync(IEnumerable<Employee> employees);
        Task<(IEnumerable<Employee> Employees, int TotalCount)> GetPaginatedAsync(
            long clientId,
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? sortBy = null,
            bool sortDescending = false);
    }
}