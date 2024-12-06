namespace JWT.Model.OTP
{
    public static class GenerateOTP
    {
        public static string GenerateOtp()
        {
            
            Random rand = new Random();
		    // fn => rand.Next(int minValue , int maxValue )  => greater than or equal to minValue and less than maxValue.
            var RandomOtp = rand.Next(10000, 100000).ToString();  // 5-digit OTP
            return RandomOtp;
        }

    }
}
