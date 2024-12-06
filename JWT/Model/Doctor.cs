using JWT;
using System.ComponentModel.DataAnnotations.Schema;

namespace Edu_plat.Model
{
	public class Doctor
	{
		// pk 
		public int DoctorId { get; set; }


		[ForeignKey("applicationUser")]
		public string? UserId { get; set; }
		public ApplicationUser? applicationUser { get; set; }
	}
}
