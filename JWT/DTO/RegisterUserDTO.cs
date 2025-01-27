using System.ComponentModel.DataAnnotations;

namespace JWT.DTO
{
	public class RegisterUserDTO
	{
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Username must contain letters only.")]

        public string UserName { get; set; }


		//[RegularExpression(@"^[a-zA-Z0-9._%+-]+@sci\.asu\.edu\.eg$", ErrorMessage = "Email must end with 'sci.asu.edu.eg'.")]

		public string Email { get; set; }  // Email Must end @'sci.asu.edu.eg

        [RegularExpression("^[A-Z](?=.[a-z])(?=.\\d)(?=.[@$!%?&#])[A-Za-z\\d@$!%*?&#]{7,19}$",
		ErrorMessage = "Password must be 8-20 characters long, starting with an uppercase letter, and include at least one lowercase letter, one digit, and one special character.")]
        public string Password { get; set; }  // with at least one uppercase letter, one lowercase letter, one digit, and one special character

		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }


	}
}
