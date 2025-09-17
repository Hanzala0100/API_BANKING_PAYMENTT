using API_BANKING_PAYMENT.Models;
using API_BANKING_PAYMENT.Respositories.IRepositories;

namespace API_BANKING_PAYMENT.Respositories
{
    public class DocumentRepository : Repository<Document>,  IDocumentRepository
    {
        public DocumentRepository(BankDbContext context) : base(context)
        {
        }
    }
    
}
