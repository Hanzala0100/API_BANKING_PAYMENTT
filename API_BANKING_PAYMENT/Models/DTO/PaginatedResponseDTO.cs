namespace API_BANKING_PAYMENT.Models.DTO
{
    public class PaginatedResponseDTO<T>
    {
        public IEnumerable<T> Data { get; set; } = new List<T>();
        public PaginationMetadataDTO Pagination { get; set; } = new PaginationMetadataDTO();
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; } = true;
    }

    public class PaginationMetadataDTO
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }
}