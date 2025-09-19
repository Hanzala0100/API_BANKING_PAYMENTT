using API_BANKING_PAYMENT.Models.DTO;

namespace API_BANKING_PAYMENT.Models.Entities
{
    public class LoginResponseModel
    {
        public UserDTO User { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime TokenExpiry { get; set; }

        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
