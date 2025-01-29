using Edu_plat.Model.Course_registeration;
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
		
		public double ? GPA { get; set; }


		//Navigational property for the Course Many side
		public ICollection<Course> courses { get; set; } = new List<Course>();


	
	}
}
