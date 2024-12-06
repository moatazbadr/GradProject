using JWT.DTO;

namespace JWT.Services
{
	public interface IMailingServices
	{
		Task SendEmailAsync(string ToMail , string Subject , string body);
	}
}
