﻿using Edu_plat.DTO.ProfileDto;
using JWT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet("Profile")]
	
		[Authorize(Roles = "Doctor,Student")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue("AppicationUserId");
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { success=false ,message="User not found"});
            }

            var userProfile = new
            {
                user.Email,
                user.UserName,
                user.PhoneNumber,
                ProfilePicture = user.profilePicture != null ? Convert.ToBase64String(user.profilePicture) : null
            };


            return Ok(userProfile);
        }
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
                    return NotFound(new {success=false ,message= "User not Found" });
                }
                var result = await _userManager.ChangePasswordAsync(user, passwordDto.CurrentPassword, passwordDto.NewPassword);
                if (result.Succeeded)
                {
                    return Ok(new {success=true ,message="Password changed succesfully"});
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return BadRequest(ModelState);

        }

        [HttpPost("profile-pic")]
		[Authorize(Roles = "Doctor , Student")]
		public async Task<IActionResult> UpdateProfilePic([FromForm] profilePic profilePicDto)
        {
            var userId = User.FindFirstValue("AppicationUserId");
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { success=false ,message= "User not found" });
            }

            if (profilePicDto.profilePicturee == null)
            {
                return BadRequest(new {success=false ,message="No file uploaded."});
            }

            using var dataStream = new MemoryStream();
            await profilePicDto.profilePicturee.CopyToAsync(dataStream);
            user.profilePicture = dataStream.ToArray();

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Ok(new {success=true ,message= "Profile picture was changed" });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return BadRequest(ModelState);
        }

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
					return NotFound(new { success=false ,message= "User not found" });
				}

				user.PhoneNumber = phoneNumberUpdateDto.NewphoneNumber;
				var result = await _userManager.UpdateAsync(user);
				if (result.Succeeded)
				{
					return Ok(new {success=true ,message= "Phone number changed successfully" });
				}

				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
			}
			return BadRequest(ModelState);
		}

	}
}
