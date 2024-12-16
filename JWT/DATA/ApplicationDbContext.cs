
using Edu_plat.Model;
using Edu_plat.Model.Course_registeration;
using Edu_plat.Model.OTP;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JWT.DATA
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{

        public ApplicationDbContext()
        {
        }

		public ApplicationDbContext(DbContextOptions options) : base(options)
        {
            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
			modelBuilder.Entity<ApplicationUser>().HasOne(a => a.Student).WithOne(s => s.applicationUser).HasForeignKey<Student>(s => s.UserId).OnDelete(DeleteBehavior.NoAction);
			modelBuilder.Entity<ApplicationUser>().HasOne(a => a.Doctor).WithOne(s => s.applicationUser).HasForeignKey<Doctor>(s => s.UserId).OnDelete(DeleteBehavior.NoAction);

		
			base.OnModelCreating(modelBuilder); 
        }
		public DbSet<Student> Students { get; set; }
       public DbSet<Doctor> Doctors {  get; set; } 

        public DbSet<TodoItems> TodoItems { get; set; }
        public DbSet<OtpVerification> OtpVerification { get; set; }
        public DbSet<TemporaryUser> TemporaryUsers { get; set; }
        public DbSet<Course> Courses { get; set; }

    }
}
