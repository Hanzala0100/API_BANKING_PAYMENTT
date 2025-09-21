namespace API_BANKING_PAYMENT.Models.DTO
{
    public class LoginResponseModel
    {
        public UserDTO User { get; set; }
        public TokenDTO Token { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
