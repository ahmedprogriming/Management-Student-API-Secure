namespace Management_Student_API_Secure.Models
{
    public class LogoutRequest
    {
        public string Email { get; set; }
        public string RefreshToken { get; set; }
    }
}
