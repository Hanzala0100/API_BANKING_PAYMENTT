using System.Linq.Expressions;
using API_BANKING_PAYMENT.Models.Entities;

namespace API_BANKING_PAYMENT.Respositories.IRepositories
{
     
        public interface IRepository<T> where T : class
        {
         Task<T> GetById(int id);
         Task<IEnumerable<T>> GetAll();
         Task<bool> Add(T entity);
         Task<bool> Update(T entity);
        Task<bool> Delete(T entity);
     }
}
