using System.ComponentModel.DataAnnotations;

namespace JWT.DTO
{
	public class LoginUserDTO
	{
     
		// Do not Need Validation here because if insert data is Invalid  will Not Login 
        public string email	{ get; set; }

		public string Password { get; set; }
	}
}
