using System.ComponentModel.DataAnnotations;

namespace JWT.DTO
{
	public class ForgotPasswordRequestDTO
	{
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@sci\.asu\.edu\.eg$", ErrorMessage = "Email must end with 'sci.asu.edu.eg'.")]
        public string Email { get; set; }
	}
}
