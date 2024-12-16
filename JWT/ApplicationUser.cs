
using Edu_plat.Model;
using Edu_plat.Model.Course_registeration;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace JWT
{
	public class ApplicationUser : IdentityUser 
	{
        public byte[]? profilePicture { get; set; }

		
		public Student Student { get; set; }
		public Doctor Doctor { get; set; }
        public ICollection<TodoItems> todoItems { get; set; } = new List<TodoItems>();

		public ICollection<Course> Courses { get; set; } =new List<Course>();

    }
}
