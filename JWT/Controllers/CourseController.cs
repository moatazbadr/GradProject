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
using Edu_plat.DTO.Course_Registration;

namespace Edu_plat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        #region Dependence Injection
        public CourseController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        #endregion

        #region Adding Course [Admin-only]

        [HttpPost("Add-course")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddCourse([FromBody] CourseRegisteration courseFromBody)
        {
            if (!ModelState.IsValid)
            {
                return Ok(new { success = false, message = "Error adding course" });
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllCourses()
        {
            var AllCourses = await _context.Courses.ToListAsync();
            return Ok(AllCourses);
        }

        #endregion

        #region Getting-courses-by-semster

        [HttpGet("Courses-semster/{sem}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> CoursesBySemster(int sem)
        {

            if (!ModelState.IsValid)
            {
                return Ok(new { success = false, message = "no semster" });
            }
            var courseBySemster = await _context.Courses.Where(x => x.Course_semster == sem && x.isRegistered == false).ToListAsync();

            var Level1_Courses = courseBySemster.Where(x => x.Course_level == 1).Select(x => new { x.CourseCode, x.CourseDescription });
            var Level2_Courses = courseBySemster.Where(x => x.Course_level == 2).Select(x => new { x.CourseCode, x.CourseDescription });
            var Level3_Courses = courseBySemster.Where(x => x.Course_level == 3).Select(x => new { x.CourseCode, x.CourseDescription });
            var Level4_Courses = courseBySemster.Where(x => x.Course_level == 4).Select(x => new { x.CourseCode, x.CourseDescription });


            var SemesterResponse = new
            {
                SmesterId = sem,
                SemesterLevels = new List<object>
                {
                    new { LevelId=1 , Level1_Courses },
                    new { LevelId=2 , Level2_Courses },
                    new { LevelId=3 , Level3_Courses },
                    new { LevelId=4 , Level4_Courses },

                }

            };


            return Ok(SemesterResponse);

        }

        #endregion

        #region Getting-courses-by-level

        [HttpGet("Courses-level/{level}")]
        public async Task<IActionResult> CoursesByLevel(int level)
        {
            int[] validLevels = { 1, 2, 3, 4 };
            var levelindex = validLevels.Where(x => x == level);
            if (!levelindex.Any())
            {
                return Ok(new { success = false, message = "Invalid Level" });
            }
            if (!ModelState.IsValid)
            {
                return Ok(new { success = false, message = "no level" });
            }
            var courseBylevel = await _context.Courses.Where(x => x.Course_level == level && x.isRegistered == false).ToListAsync();

            return Ok(courseBylevel);

        }

        #endregion

        #region Getting-Courses-by-semester & level

        [HttpGet("Courses-level/{level}/{semester}")]
        public async Task<IActionResult> CoursesBySemLevel(int level, int semester)
        {
            int[] validLevels = { 1, 2, 3, 4 };
            var levelindex = validLevels.Where(x => x == level);
            if (!levelindex.Any())
            {
                return Ok(new { success = false, message = "Invalid Level" });
            }

            if (semester != 1 || semester != 2)
            {
                return Ok(new { success = false, message = "Invalid Semester" });
            }
            
            var coursesBySemesterAndLevel = await _context.Courses
                .Where(x => x.Course_level == level && x.Course_semster == semester && x.isRegistered == false)
                .ToListAsync();

            return Ok(coursesBySemesterAndLevel);
        }


        #endregion

        #region Doctor-course-Registration [Doctor-only]
        [HttpPost("Add-Doctor-Course")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> CouresRegister(CourseRegistrationDto courseRegistrationDto)
        {
            var userId = User.FindFirstValue("AppicationUserId");
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Ok(new { success = false, message = "user not found" });
            }

            if (courseRegistrationDto.CoursesCodes == null) {

                return Ok(new { success = false, message = "Course List empty" });
            }
                List<string>successCourses = new List<string>();
                List<string>FailureCourses =  new List<string>();
            foreach(string courseCode in courseRegistrationDto.CoursesCodes)
            {
                
                //check iff the sent course is registered
                var Course= _context.Courses.FirstOrDefault(x=>x.CourseCode == courseCode);
                
                if (Course == null || Course.isRegistered==true)
                {
                    FailureCourses.Add(courseCode);
                }
                if (FailureCourses.Count > 0)
                {
                    return Ok(new { success = false, message = "Couldn't register" });
                }
                else 
                {
                   
                    Course.ApplicationUserId=userId;
                    Course.isRegistered=true;
                    _context.Courses.Update(Course);    
                    successCourses.Add(courseCode);
                }

                
            }
                await _context.SaveChangesAsync();

                return Ok(new { success=true,message="courses registered successfully"});
            
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
                return Ok(new { success = false, message = "user not found" });
            }
            else
            {
                if (_context.Courses != null)
                {
                    var courses = await _context.Courses.Where(c => c.ApplicationUserId == userId).ToListAsync();
                   // return Ok(courses);
                   List<string>Course =new List<string>();
                    foreach (var course in courses) {
                        Course.Add(course.CourseCode);
                    }
                    return Ok(Course);

                }

                return Ok(new { success = false, message = "No courses are registered to you" });

            }
        }
        #endregion

        #region Deleting Courses [Doctor only]

        
        [Authorize(Roles = "Doctor")]
        [HttpDelete("Delete-Course")]
        public async Task<IActionResult> DeleteCourse([FromBody]CourseDeletion courseDeletion)
        {
            if (courseDeletion.CourseCode == null)
            {
                return Ok(new { success = false, message = "invalid course Code" });
            }

            //check iff the sent course is registered to delete and any user cannot delete to another doctor
            var userId = User.FindFirstValue("AppicationUserId");
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return Ok(new { success = false, message = "No doctor found" });
            }
            else
            {
                var course_required = _context.Courses.FirstOrDefault(x=>x.CourseCode==courseDeletion.CourseCode && x.ApplicationUserId==userId);
                if (course_required == null)
                {
                    return Ok(new { success = false, message = "No Course found" });
                }

                else
                {
                    if (course_required.isRegistered = false || course_required.ApplicationUserId == null) {
                        return Ok(new { success = false, message = "Can not delete Course already deleted" });
                    }

                    course_required.ApplicationUserId = null;
                    course_required.isRegistered = false;
                    _context.Courses.Update(course_required);
                    _context.SaveChanges();
                    return Ok(new { success = true, message = "course Deleted sucessfully" });

                }
            }




        }
        #endregion
    }
}
