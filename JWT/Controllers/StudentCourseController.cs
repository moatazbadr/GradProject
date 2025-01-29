using JWT.DATA;
using JWT;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Edu_plat.DTO.Course_Registration;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using static System.String;

using Microsoft.EntityFrameworkCore;
namespace Edu_plat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentCourseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public StudentCourseController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpPost("Registers")]
        [Authorize(Roles ="Student")]
        public async Task<IActionResult> Register(CourseRegistrationDto registrationDto)
        {
        
               
                var userId = User.FindFirstValue("AppicationUserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return Ok(new { success = false, message = "Invalid Token: User not found" });
                }

                var student = await _context.Students
                    .Include(s => s.courses)
                    .FirstOrDefaultAsync(s => s.UserId == userId);

                if (student == null)
                {
                    return Ok(new { success = false, message = "Student not found" });
                }

                if (registrationDto.CoursesCodes == null || !registrationDto.CoursesCodes.Any())
                {
                    return Ok(new { success = false, message = "Invalid Registration Data" });
                }

                var coursesToRegister = await _context.Courses
                    .Where(c => registrationDto.CoursesCodes.Contains(c.CourseCode))
                    .ToListAsync();

                if (!coursesToRegister.Any())
                {
                    return Ok(new { success = false, message = "No matching courses found" });
                }

                foreach (var course in coursesToRegister)
                {
                    if (!student.courses.Any(c => c.Id == course.Id))
                    {
                        student.courses.Add(course);
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Course registration successful" });
            }
            
        }





    }