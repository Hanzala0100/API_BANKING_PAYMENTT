using API_BANKING_PAYMENT.Models.Entities;

namespace API_BANKING_PAYMENT.Respositories.IRepositories
{
    public interface IClientRepository : IRepository<Client>
    {
        Task<IEnumerable<Client>> GetClientsByBankId(long bankId);
        Task<Client> GetClientByRegisterationNumber(string registerationNumber);
        Task<Client> AddClientAsync(Client client);
        Task<IEnumerable<Client>> GetClientsAllAsync(long id);
        // Docs
        Task<IEnumerable<Document>> GetClientDocumentsAsync(long clientId);
        Task<bool> ClientHasDocumentsAsync(long clientId);
        Task<int> GetClientDocumentCountAsync(long clientId);

        // Verification 
        Task<IEnumerable<Client>> GetClientsByVerificationStatusAsync(string verificationStatus);
        Task<IEnumerable<Client>> GetClientsWithPendingVerificationAsync();
        Task<IEnumerable<Client>> GetClientsByBankAndStatusAsync(long bankId, string verificationStatus);
        Task<bool> UpdateClientVerificationStatusAsync(long clientId, string verificationStatus, long verifiedBy);

        // Client details
        Task<Client> GetClientWithDetailsAsync(long clientId);
    }
}