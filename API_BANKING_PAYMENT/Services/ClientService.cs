using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;
using API_BANKING_PAYMENT.Respositories.IRepositories;
using API_BANKING_PAYMENT.Services.IServices;
using AutoMapper;
using Microsoft.AspNetCore.Http;

namespace API_BANKING_PAYMENT.Services
{
    public class ClientService : IClientService
    {
        private readonly IDocumentService _documentService;
        private readonly IClientRepository _clientRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ClientService> _logger;

        public ClientService(
            IDocumentService documentService,
            IClientRepository clientRepository,
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<ClientService> logger)
        {
            _documentService = documentService;
            _clientRepository = clientRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
        }


        public async Task<BaseResponseDTO<DocumentDTO>> UploadClientDocumentAsync(long clientId, IFormFile file, long uploadedBy, string docType)
        {
            try
            {
                var client = await _clientRepository.GetById(clientId);
                if (client == null)
                    return BaseResponseDTO<DocumentDTO>.ErrorResult("Client not found");

                if (file == null || file.Length == 0)
                    return BaseResponseDTO<DocumentDTO>.ErrorResult("File is required");

                if (!client.BankId.HasValue)
                    return BaseResponseDTO<DocumentDTO>.ErrorResult("Bank ID is missing for this client");


                var bankId = client.BankId.Value;

                var result = await _documentService.UploadDocumentAsync(file, uploadedBy, bankId, clientId, docType);

                if (result.Success)
                {
                    _logger.LogInformation("Document uploaded successfully for client ID: {ClientId}, Type: {DocType}",
                        clientId, docType);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document for client ID: {ClientId}", clientId);
                return BaseResponseDTO<DocumentDTO>.ErrorResult("Error uploading document", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<IEnumerable<DocumentDTO>>> GetClientDocumentsAsync(long clientId)
        {
            try
            {
                var client = await _clientRepository.GetById(clientId);
                if (client == null)
                    return BaseResponseDTO<IEnumerable<DocumentDTO>>.ErrorResult("Client not found");

                var documents = await _clientRepository.GetClientDocumentsAsync(clientId);
                var documentDTOs = _mapper.Map<IEnumerable<DocumentDTO>>(documents);

                return BaseResponseDTO<IEnumerable<DocumentDTO>>.SuccessResult(documentDTOs, "Client documents retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving documents for client ID: {ClientId}", clientId);
                return BaseResponseDTO<IEnumerable<DocumentDTO>>.ErrorResult("Error retrieving documents", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<DocumentDTO>> GetClientDocumentByIdAsync(long clientId, long documentId)
        {
            try
            {
                var client = await _clientRepository.GetById(clientId);
                if (client == null)
                    return BaseResponseDTO<DocumentDTO>.ErrorResult("Client not found");

                var document = await _documentService.GetDocumentByIdAsync(documentId);

                if (document.Success && document.Data?.ClientId != clientId)
                    return BaseResponseDTO<DocumentDTO>.ErrorResult("Document not found for this client");

                return document;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document {DocumentId} for client ID: {ClientId}", documentId, clientId);
                return BaseResponseDTO<DocumentDTO>.ErrorResult("Error retrieving document", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<bool>> DeleteClientDocumentAsync(long clientId, long documentId)
        {
            try
            {
                var client = await _clientRepository.GetById(clientId);
                if (client == null)
                    return BaseResponseDTO<bool>.ErrorResult("Client not found");

                var document = await _documentService.GetDocumentByIdAsync(documentId);
                if (!document.Success || document.Data?.ClientId != clientId)
                    return BaseResponseDTO<bool>.ErrorResult("Document not found for this client");

                var result = await _documentService.DeleteDocumentAsync(documentId);

                if (result.Success)
                {
                    _logger.LogInformation("Document {DocumentId} deleted for client ID: {ClientId}", documentId, clientId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document {DocumentId} for client ID: {ClientId}", documentId, clientId);
                return BaseResponseDTO<bool>.ErrorResult("Error deleting document", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<DocumentDTO>> UpdateClientDocumentAsync(long clientId, long documentId, IFormFile newFile)
        {
            try
            {
                var client = await _clientRepository.GetById(clientId);
                if (client == null)
                    return BaseResponseDTO<DocumentDTO>.ErrorResult("Client not found");

                var existingDocument = await _documentService.GetDocumentByIdAsync(documentId);
                if (!existingDocument.Success || existingDocument.Data?.ClientId != clientId)
                    return BaseResponseDTO<DocumentDTO>.ErrorResult("Document not found for this client");

                var result = await _documentService.UpdateDocumentAsync(documentId, newFile);

                if (result.Success)
                {
                    _logger.LogInformation("Document {DocumentId} updated for client ID: {ClientId}", documentId, clientId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document {DocumentId} for client ID: {ClientId}", documentId, clientId);
                return BaseResponseDTO<DocumentDTO>.ErrorResult("Error updating document", new List<string> { ex.Message });
            }
        }

        public async Task<BaseResponseDTO<IEnumerable<DocumentDTO>>> GetClientDocumentsByTypeAsync(long clientId, string docType)
        {
            try
            {
                var client = await _clientRepository.GetById(clientId);
                if (client == null)
                    return BaseResponseDTO<IEnumerable<DocumentDTO>>.ErrorResult("Client not found");

                var documents = await _clientRepository.GetClientDocumentsAsync(clientId);
                var filteredDocuments = documents.Where(d => d.DocType == docType);
                var documentDTOs = _mapper.Map<IEnumerable<DocumentDTO>>(filteredDocuments);

                return BaseResponseDTO<IEnumerable<DocumentDTO>>.SuccessResult(documentDTOs, $"{docType} documents retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving {DocType} documents for client ID: {ClientId}", docType, clientId);
                return BaseResponseDTO<IEnumerable<DocumentDTO>>.ErrorResult("Error retrieving documents", new List<string> { ex.Message });
            }
        }
    }
}