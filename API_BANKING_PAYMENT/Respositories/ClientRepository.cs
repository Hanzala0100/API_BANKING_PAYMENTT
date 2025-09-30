using API_BANKING_PAYMENT.Models;
using API_BANKING_PAYMENT.Models.Entities;
using API_BANKING_PAYMENT.Models.Enum;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace API_BANKING_PAYMENT.Respositories
{
    public class ClientRepository : Repository<Client>, IClientRepository
    {
        private readonly BankDbContext _context;

        public ClientRepository(BankDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Client>> GetClientsByBankId(long bankId)
        {
            return await _context.Clients
                .Where(cl => cl.BankId == bankId)
                .Include(c => c.Bank)
                .ToListAsync();
        }


        public async Task<IEnumerable<Client>> GetClientsAllAsync(long id)
        {
            return await _context.Clients
                .Where(cl => cl.BankId == id)
                .Include(c => c.Bank)
                .ToListAsync();
        }

        public async Task<Client> GetClientByRegisterationNumber(string registerationNumber)
        {
            return await _context.Clients
                .FirstOrDefaultAsync(c => c.RegisterationNumber == registerationNumber);
        }

        public async Task<IEnumerable<Document>> GetClientDocumentsAsync(long clientId)
        {
            return await _context.Documents
                .Where(d => d.ClientId == clientId)
                .Include(d => d.UploadedByNavigation)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Client>> GetClientsWithPendingVerificationAsync()
        {
            return await _context.Clients
                .Where(c => c.VerificationStatus == VerificationStatus.Pending ||
                           c.VerificationStatus == VerificationStatus.InReview)
                .Include(c => c.Bank)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Client>> GetClientsByVerificationStatusAsync(string verificationStatus)
        {
            return await _context.Clients
                .Where(c => c.VerificationStatus == verificationStatus)
                .Include(c => c.Bank)
                .Include(c => c.Users)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Client> GetClientWithDetailsAsync(long clientId)
        {
            return await _context.Clients
                .Include(c => c.Bank)
                .Include(c => c.Users)
                .Include(c => c.Beneficiaries)
                .Include(c => c.Employees)
                .Include(c => c.Payments)
                .FirstOrDefaultAsync(c => c.ClientId == clientId);
        }

        public async Task<bool> ClientHasDocumentsAsync(long clientId)
        {
            return await _context.Documents
                .AnyAsync(d => d.ClientId == clientId);
        }

        public async Task<int> GetClientDocumentCountAsync(long clientId)
        {
            return await _context.Documents
                .CountAsync(d => d.ClientId == clientId);
        }

    

        public async Task<Client> AddClientAsync(Client client)
        {
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            return client;
        }
        public async Task<bool> UpdateClientVerificationStatusAsync(long clientId, string verificationStatus, long verifiedBy)
        {
            var client = await _context.Clients.FindAsync(clientId);
            if (client == null)
                return false;

            client.VerificationStatus = verificationStatus;
            client.VerifiedBy = verifiedBy;
            client.VerifiedAt = DateTime.UtcNow;

            _context.Clients.Update(client);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Client>> GetClientsByBankAndStatusAsync(long bankId, string verificationStatus)
        {
            return await _context.Clients
                .Where(c => c.BankId == bankId && c.VerificationStatus == verificationStatus)
                .Include(c => c.Users)
                .OrderBy(c => c.ClientName)
                .ToListAsync();
        }
    }
}