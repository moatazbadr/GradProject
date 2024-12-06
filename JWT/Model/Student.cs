using JWT;
using System.ComponentModel.DataAnnotations.Schema;

namespace Edu_plat.Model
{
	public class Student
	{
		// pk 
		public int StudentId { get; set; }

		
		[ForeignKey("applicationUser")]
		public string? UserId { get; set; }
		public ApplicationUser? applicationUser { get; set; }
	}
}
