namespace API_BANKING_PAYMENT.Models.DTO
{
    public class BaseResponseDTO<T>
    {
 
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public static BaseResponseDTO<T> SuccessResult(T data, string message = "Operation completed successfully")
        {
            return new BaseResponseDTO<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static BaseResponseDTO<T> ErrorResult(string errorMessage, List<string> errors = null)
        {
            return new BaseResponseDTO<T>
            {
                Success = false,
                Message = errorMessage,
                Errors = errors ?? new List<string>()
            };
        }
    }
    
}