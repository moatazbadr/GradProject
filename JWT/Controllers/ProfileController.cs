using Edu_plat.DTO.ProfileDto;
using JWT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using System.Security.Claims;

// api change phone (constrain on phone)
namespace Edu_plat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        #region Getting profile [student && Doctors]
        [HttpGet("Profile")]

        [Authorize(Roles = "Doctor,Student")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue("AppicationUserId");
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            var userProfile = new
            {
                user.Email,
                user.UserName
            };


            return Ok(userProfile);
        }
        #endregion

        #region  changing Password [student && Doctors]
        [HttpPost("changePassword")]
        [Authorize(Roles = "Doctor , Student")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangepasswordDto passwordDto)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue("AppicationUserId");
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not Found" });
                }
                var result = await _userManager.ChangePasswordAsync(user, passwordDto.CurrentPassword, passwordDto.NewPassword);
                if (result.Succeeded)
                {
                    return Ok(new { success = true, message = "Password changed succesfully" });
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return BadRequest(ModelState);

        }
        #endregion

        #region upload profile-pic
        [HttpPost("profile-pic")]
        [Authorize(Roles = "Doctor, Student")]
        public async Task<IActionResult> UpdateProfilePic([FromForm] profilePic profilePicDto)
        {
            var userId = User.FindFirstValue("AppicationUserId");
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            if (profilePicDto.profilePicturee == null)
            {
                return BadRequest(new { success = false, message = "No file uploaded." });
            }

            // Validate file size (max 2 MB)
            if (profilePicDto.profilePicturee.Length > 2 * 1024 * 1024)
            {
                return BadRequest(new { success = false, message = "File size exceeds the limit of 2MB." });
            }

            // Validate file extension (only jpeg, jpg, png)
            var fileExtension = Path.GetExtension(profilePicDto.profilePicturee.FileName).ToLower();
            var allowedExtensions = new[] { ".jpeg", ".jpg", ".png" };
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new { success = false, message = "Invalid file type. Only JPG, JPEG, and PNG files are allowed." });
            }

            using var dataStream = new MemoryStream();
            await profilePicDto.profilePicturee.CopyToAsync(dataStream);
            user.profilePicture = dataStream.ToArray();

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Ok(new { success = true, message = "Profile picture was changed" });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return BadRequest(ModelState);
        } 
        #endregion

        #region Changing the phone number [student && Doctors]

        [HttpPost("changePhoneNumber")]
        [Authorize(Roles = "Doctor,Student")]
        public async Task<IActionResult> ChangePhoneNumber([FromBody] PhoneNumberDto phoneNumberUpdateDto)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue("AppicationUserId");
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                user.PhoneNumber = phoneNumberUpdateDto.NewphoneNumber;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return Ok(new { success = true, message = "Phone number changed successfully" });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return BadRequest(ModelState);
        }

        #endregion

        #region Get profile Picture [student && Doctors]
        [HttpGet("profile-pic")]
        [Authorize(Roles = "Doctor , Student")]
        public async Task<IActionResult> GetProfilePic()
        {
            var userId = User.FindFirstValue("AppicationUserId");
            var user = await _userManager.FindByIdAsync(userId);
            if (user.profilePicture == null)
            {
                return Ok(new {profilephoto =user.profilePicture});
            }
            var profilepic = Convert.ToBase64String(user.profilePicture);

            return Ok(new { profilephoto = user.profilePicture });

        }


        #endregion

        #region Getting phoneNumber [students && Doctors]
        [HttpGet("phone-number")]
        [Authorize(Roles = "Doctor,Student")]
        public async Task<IActionResult> GetPhoneNumber()
        {
            var userId = User.FindFirstValue("AppicationUserId");
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }
            if (user.PhoneNumber==null || user.PhoneNumber.Length==0) {
                return Ok(new { phoneNumber=user.PhoneNumber});
            }
            return Ok(new { success = true, phoneNumber = user.PhoneNumber });
        } 
        #endregion
    }
}
