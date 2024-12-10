namespace Edu_plat.Model.OTP
{
    public class OtpVerification
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Otp { get; set; }
        public DateTime ExpirationTime { get; set; }
        public string? Purpose { get; set; }
        public bool IsVerified { get; set; }
    }
}
