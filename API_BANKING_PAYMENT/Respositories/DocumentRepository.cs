using API_BANKING_PAYMENT.Models;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using API_BANKING_PAYMENT.Models.Entities;

namespace API_BANKING_PAYMENT.Respositories
{
    public class DocumentRepository : Repository<Document>,  IDocumentRepository
    {
        public DocumentRepository(BankDbContext context) : base(context)
        {
        }
    }
    
}
