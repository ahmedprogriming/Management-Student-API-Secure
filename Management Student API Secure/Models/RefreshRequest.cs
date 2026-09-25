namespace Management_Student_API_Secure.Models
{
    public class RefreshRequest
    {
        public string RefreshToken { get; set; }
        public string Email { get; set; }
    }
}
