namespace API_BANKING_PAYMENT.Models.DTO
{

    public class LoginTokenRepsonse()
    {
        public UserDTO User { get; set; }
        public TokenDTO Token { get; set; }
    }
    public class LoginResponseModel : BaseResponseDTO<LoginTokenRepsonse>
    {
        
    }
}
