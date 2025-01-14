using System.ComponentModel.DataAnnotations;

namespace JWT.DTO
{
	public class ResetPasswordRequestDTO
	{

        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,20}$",
       ErrorMessage = "Password must be 8-20 characters long, with at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
        public string NewPassword { get; set; }
		
		[Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.") ]
		public string ConfirmPassword { get; set; }
		
	}
}
