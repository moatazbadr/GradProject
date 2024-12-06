using System.ComponentModel.DataAnnotations;

namespace JWT.DTO
{
	public class MailRequsetDTO
	{
	
		public string ToEmail { get; set; }
		
		public string Subject { get; set; }
	
		public string Body { get; set; }

	}
}
