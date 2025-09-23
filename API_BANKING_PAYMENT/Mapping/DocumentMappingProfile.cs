using API_BANKING_PAYMENT.Models.DTO;
using API_BANKING_PAYMENT.Models.Entities;
using AutoMapper;

public class DocumentProfile : Profile
{
    public DocumentProfile()
    {
        CreateMap<Document, DocumentDTO>();

        CreateMap<DocumentDTO, Document>()
            .ForMember(dest => dest.DocumentId, opt => opt.Ignore()) 
            .ForMember(dest => dest.UploadedAt, opt => opt.Ignore()); 
    }
}
