using System.ComponentModel.DataAnnotations;

namespace Edu_plat.DTO
{
	public class TemporaryUserDTO
	{
		public string UserName { get; set; }


		[RegularExpression(@"^[a-zA-Z0-9._%+-]+@sci\.asu\.edu\.eg$", ErrorMessage = "Email must end with 'sci.asu.edu.eg'.")]

		public string Email { get; set; }  // Email Must end @'sci.asu.edu.eg

		[RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,15}$",
		ErrorMessage = "Password must be 8-20 characters long, with at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
		public string Password { get; set; }  // with at least one uppercase letter, one lowercase letter, one digit, and one special character
	
		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }
	}
}
