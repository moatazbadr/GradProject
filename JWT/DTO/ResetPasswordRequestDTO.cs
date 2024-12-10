using System.ComponentModel.DataAnnotations;

namespace JWT.DTO
{
	public class ResetPasswordRequestDTO
	{
		//public string Email { get; set; }
		
		public string NewPassword { get; set; }
		
		[Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.") ]
		public string ConfirmPassword { get; set; }
		// public string Otp { get; set; } // OTP from  the user
		//
	}
}
