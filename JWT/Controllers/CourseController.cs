using JWT.DATA;
using JWT;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Edu_plat.Model.Course_registeration;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;

namespace Edu_plat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CourseController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }


        #region Adding Course [Admin-only]

        [HttpPost("Add-course")]
        public async Task<IActionResult> AddCourse([FromBody] Course courseFromBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Error adding course" });
            }
            var course = new Course()
            {
                CourseCode = courseFromBody.CourseCode,
                CourseDescription = courseFromBody.CourseDescription,
                Course_hours = courseFromBody.Course_hours,
                Course_degree = courseFromBody.Course_degree,
                isRegistered = false,
                Course_level = courseFromBody.Course_level,
                Course_semster = courseFromBody.Course_semster,
            };
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "course added successfully" });

        }

        #endregion


        #region Getting-all-courses [Admin-only]

        [HttpGet("Get-all-courses")]
        public async Task<IActionResult> GetAllCourses()
        {
            var AllCourses = await _context.Courses.ToListAsync();
            return Ok(AllCourses);
        }

        #endregion

        #region Getting-courses-by-semster

        [HttpGet("Courses-semster/{sem}")]
        public async Task<IActionResult> CoursesBySemster(int sem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "no semster" });
            }
            var courseBySemster = await _context.Courses.Where(x => x.Course_semster == sem && x.isRegistered == false).ToListAsync();

            return Ok(courseBySemster);

        }

        #endregion

        #region Getting-courses-by-level

        [HttpGet("Courses-level/{level}")]
        public async Task<IActionResult> CoursesByLevel(int level)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "no level" });
            }
            var courseBylevel = await _context.Courses.Where(x => x.Course_level == level && x.isRegistered == false).ToListAsync();

            return Ok(courseBylevel);

        }

        #endregion

        #region Getting-Courses-by-semester & level

        [HttpGet("Courses-level/{level}/{semester}")]
        public async Task<IActionResult> CoursesBySemLevel(int level, int semester)
        {
            var coursesBySemesterAndLevel = await _context.Courses
                .Where(x => x.Course_level == level && x.Course_semster == semester && x.isRegistered == false)
                .ToListAsync();

            return Ok(coursesBySemesterAndLevel);
        }


        #endregion

        #region Doctor-course-Registration [Doctor-only]
        [HttpPost("Add-Doctor-Course/{course_id}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> CouresRegister(int course_id)
        {
            var userId = User.FindFirstValue("AppicationUserId");
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { success = false, message = "user not found" });
            }
            else
            {
                var course_required = await _context.Courses.FindAsync(course_id);
                if (course_required == null)
                {
                    return NotFound(new { success = false, message = "No course was found" });
                }
                else
                {
                    course_required.ApplicationUserId = userId;
                    course_required.isRegistered = true;
                    _context.Courses.Update(course_required);
                    await _context.SaveChangesAsync();
                    return Ok(new { success = true, message = "course registered sucessfully" });
                }
            }
        }




        #endregion

        #region Getting-courses-assigned-to-doctor [Doctor-only] 
        [HttpGet("Get-doctor-courses")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetMyCourses()
        {
            var userId = User.FindFirstValue("AppicationUserId");
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { success = false, message = "user not found" });
            }
            else
            {
                if (_context.Courses != null)
                {
                    var courses = await _context.Courses.Where(c => c.ApplicationUserId == userId).ToListAsync();
                    return Ok(courses);
                }

                return BadRequest(new { success = false, message = "No courses are registered to you" });

            }




            #endregion

        }
    }
}
