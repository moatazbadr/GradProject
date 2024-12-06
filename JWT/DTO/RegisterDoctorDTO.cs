using System.ComponentModel.DataAnnotations;

namespace JWT.DTO
{
	public class RegisterDoctorDTO
	{
		    public string UserName { get; set; }

		 [RegularExpression(@"^[a-zA-Z0-9._%+-]+@sci\.asu\.edu\.eg$", ErrorMessage = "Email must end with 'sci.asu.edu.eg'.")]

		public string Email { get; set; }
		[RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,20}$",
	   ErrorMessage = "Password must be 8-20 characters long, with at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
		public string Password { get; set; }
		
	}
}
