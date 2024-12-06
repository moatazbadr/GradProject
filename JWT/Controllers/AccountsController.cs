using Edu_plat.DTO;
using Edu_plat.Model;
using Edu_plat.Model.OTP;
using JWT.DATA;
using JWT.DTO;
using JWT.Model.OTP;
using JWT.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Generators;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;


namespace JWT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]



    public class AccountsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMailingServices _mailService;
        private readonly ApplicationDbContext _context;


        // Declare a dictionary where key is a string (email), and value is a tuple (OTP, expiration time, TemporaryUserDTO user)
        private static readonly Dictionary<string, (string Otp, DateTime ExpirationTime, TemporaryUserDTO TempUser)> _otpStore = new Dictionary<string, (string, DateTime, TemporaryUserDTO)>();

        // Declare a dictionary where key is a string (email), and value is a tuple (OTP, expiration time)
        private static readonly Dictionary<string, (string Otp, DateTime ExpirationTime)> _otpStoreFR = new Dictionary<string, (string, DateTime)>();



        #region Dependency Injection
        public AccountsController(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            RoleManager<IdentityRole> roleManager,
            IMailingServices mailService,
             ApplicationDbContext context

            )
        {
            _userManager = userManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _mailService = mailService;
            _context = context;
        }
        #endregion

        #region Register Noe verify

        //[HttpPost("Register")]
        //public async Task<IActionResult> Register([FromBody] RegisterUserDTO dto)
        //{
        //	if (ModelState.IsValid)
        //	{
        //		// save DB 
        //		ApplicationUser applicationUser = new ApplicationUser
        //		{
        //			UserName = dto.UserName,
        //			Email = dto.Email,

        //		};
        //		IdentityResult result = await _userManager.CreateAsync(applicationUser, dto.Password);
        //		if (result.Succeeded)
        //			return Ok("Dn Register");

        //		foreach (var item in result.Errors)
        //		{
        //			ModelState.AddModelError("Password", item.Description);
        //		}
        //	}

        //	return BadRequest(ModelState);

        //} 
        #endregion

        #region Register 
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDTO dto)
        {
            // check all Data Entire is True or False 
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            // cjeck exmail Exist or Not if exist not can Register the same Email  
            var existingUser = await _userManager.Users.AnyAsync(u => u.Email == dto.Email);
            if (existingUser)
                return BadRequest("Email already exists");

            // check Email store in TemporaryUser or Not 
            var existingTempUser = await _context.TemporaryUsers
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            // if email exist in tempUser
            if (existingTempUser != null)
            {
                // check Otp which existingTemp if otp exist and not expire send same otp again 
                var existingOtp = await _context.OtpVerification
                    .FirstOrDefaultAsync(o => o.Email == dto.Email && DateTime.UtcNow < o.ExpirationTime);

                if (existingOtp != null)
                {
                    await _mailService.SendEmailAsync(dto.Email, "Your OTP Is On Its Way!",
                    $@"
                   <p>Hello {dto.UserName},</p>
                  <p>We noticed that you requested an OTP again. Don't worry, your previous code is still valid!</p>
                  <p>Here is your One-Time Password (OTP):</p>
                  <h2 style='color: #4CAF50;'>{existingOtp.Otp}</h2>
                  <p><strong>Note:</strong> This code is valid for the next <strong>5 minutes</strong>.</p>
                   <p>If you did not request this, please ignore this email. Your account is secure.</p>
                    <p>Take care,</p>
                  <p><strong>The EduPlat Team</strong></p>");


                    return Ok("OTP has been resent to your email.");
                }
                else
                {
                    // if otp finished expire 

                    string otp = GenerateOTP.GenerateOtp();
                    DateTime expirationTime = DateTime.UtcNow.AddMinutes(5);

                    var otpVerification = new OtpVerification
                    {
                        Email = dto.Email,
                        Otp = otp,
                        ExpirationTime = expirationTime
                    };
                    await _context.OtpVerification.AddAsync(otpVerification);
                    await _context.SaveChangesAsync();

                    await _mailService.SendEmailAsync(dto.Email, "Your OTP for Registration",
                  $"<p>Dear {dto.UserName},</p>" +
                 $"<p>Welcome! We're excited to have you join us.</p>" +
                 $"<p>Your One-Time Password (OTP) for email verification is:</p>" +
                $"<h2 style='color: #4CAF50;'>{otp}</h2>" +
                 $"<p>This code is valid for the next 5 minutes, so please use it promptly.</p>" +
                  $"<p>If you have any questions, feel free to reach out to us.</p>" +
                    $"<p>Best wishes,</p>" +
                    $"<p>EduPlat</p>");


                    return Ok("OTP has been resent to your email.");
                }
            }

            // if not Exist User => Register User 
            var tempUser = new TemporaryUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                PasswordHash = dto.Password


            };
            await _context.TemporaryUsers.AddAsync(tempUser);

            // Generate OTP
            string newOtp = GenerateOTP.GenerateOtp();
            DateTime newExpirationTime = DateTime.UtcNow.AddMinutes(5);

            // save data in table otp 
            var otpVerificationNew = new OtpVerification
            {
                Email = dto.Email,
                Otp = newOtp,
                ExpirationTime = newExpirationTime
            };
            await _context.OtpVerification.AddAsync(otpVerificationNew);
            await _context.SaveChangesAsync();
            await _mailService.SendEmailAsync(dto.Email, "Welcome to Our Platform!",
    $"<p>Hi {dto.UserName},</p>" +
    $"<p>We’re thrilled to have you here! To complete your registration, please verify your email using the code below:</p>" +
    $"<h2 style='color: #4CAF50;'>{newOtp}</h2>" +
    $"<p>This code is valid for the next 5 minutes, so make sure to use it soon.</p>" +
    $"<p>Need help? Feel free to reach out—we’re here for you!</p>" +
    $"<p>Warm regards,</p>" +
    $"<p>The EduPlat Team</p>");


            return Ok("Registration successful. A verification code has been sent to your email.");
        }

        #endregion

        #region VerifyAccount

        #region VerifyAccount

        [HttpPost("VerifyEmail")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyOtpDTO dto)
        {
            // check otp 
            var otpRecord = await _context.OtpVerification.FirstOrDefaultAsync(o => o.Otp == dto.Otp);
            if (otpRecord == null)
                return BadRequest("Invalid OTP.");

            // check expire Time 
            if (DateTime.UtcNow > otpRecord.ExpirationTime)
                return BadRequest("OTP has expired.");

            // find tempUser 
            var tempUser = await _context.TemporaryUsers.FirstOrDefaultAsync(u => u.Email == otpRecord.Email);
            if (tempUser == null)
                return BadRequest("Temporary user not found.");

            // delete otp 
            _context.OtpVerification.Remove(otpRecord);
            await _context.SaveChangesAsync();


            // create new User 
            var newUser = new ApplicationUser
            {
                UserName = tempUser.UserName,
                Email = tempUser.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(newUser, tempUser.PasswordHash);
            if (!result.Succeeded)
                return BadRequest(result.Errors);
            if (result.Succeeded)
            {
                newUser.EmailConfirmed = true;
                await _userManager.UpdateAsync(newUser); // Update the user in the database

                // Assign the "Student" role
                var roleExist = await _roleManager.RoleExistsAsync("Student");
                if (!roleExist)
                {
                    // If "Student" role doesn't exist, create it
                    await _roleManager.CreateAsync(new IdentityRole("Student"));
                }

                // Assign the "Student" role to the user
                await _userManager.AddToRoleAsync(newUser, "Student");

                return Ok(new { success = true, message = "Email verified successfully and user created." });
            }
            // delete UserTemp
            _context.TemporaryUsers.Remove(tempUser);
            await _context.SaveChangesAsync();

            return Ok("Email verified successfully and user account created.");
        }

        #endregion


        #endregion
        #region Login

        [HttpPost("Login")]

        public async Task<IActionResult> Login([FromBody] LoginUserDTO dto)
        {
            if (ModelState.IsValid)
            {
                // check => account Exist

                var account = await _userManager.FindByEmailAsync(dto.email);
                if (account == null)
                    return BadRequest(new { success = false, message = "Invalid password or email " });


                var checkPass = await _userManager.CheckPasswordAsync(account, dto.Password);
                if (checkPass)
                {

                    var UserClaims = new List<Claim>();



                    UserClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                    UserClaims.Add(new Claim("AppicationUserId", account.Id));
                    UserClaims.Add(new Claim("ApplicationUserName", account.UserName));



                    var Roles = await _userManager.GetRolesAsync(account);
                    foreach (var RoleName in Roles)
                        UserClaims.Add(new Claim(ClaimTypes.Role, RoleName));
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));

                    SigningCredentials signingCred = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);



                    JwtSecurityToken jwtSecurityToken = new JwtSecurityToken
                    (
                         issuer: _configuration["JWT:IssuerIP"],
                        audience: _configuration["JWT:audienceIP"],
                        expires: DateTime.Now.AddYears(1),
                        claims: UserClaims,
                        signingCredentials: signingCred
                    );
                    var roles = await _userManager.GetRolesAsync(account);

                    string userRole = roles.FirstOrDefault();
                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                        roles = roles,
                        expiration = DateTime.Now.AddYears(1)
                    });
                }
                else
                {

                    return BadRequest("User or Password inValid");
                }
            }
            else
                return BadRequest("InValid Data");
        }
        #endregion

        #region RegisterAdmin

        //// Admin registration (for demo purposes)
        [HttpPost("RegisterAdmin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterUserDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var admin = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(admin, dto.Password);
            if (result.Succeeded)
            {
                // Assign Admin role
                var roleExist = await _roleManager.RoleExistsAsync("Admin");
                if (!roleExist)
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                }
                await _userManager.AddToRoleAsync(admin, "Admin");
                return Ok("Admin user created successfully.");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);
        }
        #endregion

        #region RegisterDoctor 
        // Register Doctor (Only Admin can use this endpoint)
        [HttpPost("RegisterDoctor")]
        [Authorize(Roles = "Admin")] // Only Admins can register Doctors
        public async Task<IActionResult> RegisterDoctor([FromBody] RegisterDoctorDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var doctor = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(doctor, dto.Password);
            // Save the userId for later use
            var userId = doctor.Id;
            // Step 2: Create and Save a Doctor linked to the ApplicationUser
            var DoctorObj = new Doctor
            {
                UserId = userId, // Foreign key linking to ApplicationUser
                applicationUser = doctor

            };
            _context.Set<Doctor>().Add(DoctorObj);
            _context.SaveChanges();
            if (result.Succeeded)
            {
                // Assign Doctor role
                var roleResult = await _userManager.AddToRoleAsync(doctor, "Doctor");
                if (!roleResult.Succeeded)
                {
                    return StatusCode(500, "Failed to assign Doctor role.");
                }

                return Ok(new { Message = "Doctor registered successfully." });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);
        }


        #endregion

        #region SendEmail
        [HttpPost("SendEmail")]
        public async Task<IActionResult> sendMail([FromForm] MailRequsetDTO dto)
        {

            await _mailService.SendEmailAsync(dto.ToEmail, dto.Subject, dto.Body);
            return Ok();
        }
        #endregion

        #region ForgetPassword
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid request.");
            // Remove Otps store in Database (when ask new otp )
            var existingOtps = _context.OtpVerification
                  .Where(o => o.Email == model.Email && o.Purpose == "ResetPassword");
            _context.OtpVerification.RemoveRange(existingOtps);

            // Check if the user exists based on the email
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest("User not found.");

            // Generate OTP
            string otp = GenerateOTP.GenerateOtp();

            // Set OTP expiration time
            DateTime expirationTime = DateTime.UtcNow.AddMinutes(5);

            // Store OTP in the database
            var otpVerification = new OtpVerification
            {
                Email = model.Email,
                Otp = otp,
                Purpose = "ResetPassword", // Define purpose for OTP
                ExpirationTime = expirationTime
            };

            await _context.OtpVerification.AddAsync(otpVerification);
            await _context.SaveChangesAsync();

            // Send OTP via email
            var emailBody = $@"
	      <p>Hi there,</p>
	      <p>We received a request to reset your password, and we're here to help!</p>
	      <p>Your OTP (One-Time Password) to reset your password is:</p>
	      <h2 style='color: #2E86C1;'>{otp}</h2>
	      <p>This code is valid for the next <strong>5 minutes</strong>.</p>
	      <p>If you didn't request a password reset, no worries—your account is still safe. You can simply ignore this email.</p>
	      <p>Take care,</p>
	      <p><strong>The EduPlat Team</strong></p>";

            await _mailService.SendEmailAsync(model.Email, "Password Reset OTP", emailBody);

            return Ok("OTP sent to your email.");
        }


        #endregion

        #region ResetPassword 

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid request.");

            // Retrieve the OTP record from the database
            var otpRecord = await _context.OtpVerification
                .FirstOrDefaultAsync(o => o.Otp == model.Otp && o.Email == model.Email && o.Purpose == "ResetPassword");

            // Check if OTP exists
            if (otpRecord == null)
            {
                return BadRequest("Invalid OTP.");
            }

            // Check if OTP has expired
            if (DateTime.UtcNow > otpRecord.ExpirationTime)
            {
                return BadRequest("OTP has expired.");
            }

            // Retrieve the user by email
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            // Generate a password reset token for the user
            string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Reset the user's password using the reset token
            var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);
            if (!resetResult.Succeeded)
            {
                return BadRequest("Password reset failed.");
            }

            // Delete the OTP record after successful password reset
            _context.OtpVerification.Remove(otpRecord);



            await _context.SaveChangesAsync();

            return Ok("Password reset successful.");
        }
    }
}

//[HttpPost("forgot-password")]
//public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO model)
//{
//	if (!ModelState.IsValid)
//		return BadRequest("Invalid request.");

//	var user = await _userManager.FindByEmailAsync(model.Email);
//	if (user == null)
//		return BadRequest("User not found.");

//	// Normalize the email address to lower case to avoid case sensitivity issues
//	string emailKey = model.Email.ToLower();

//	// Generate OTP
//	string otp = GenerateOTP.GenerateOtp();

//	// Generate password reset token
//	string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

//	// Store OTP, token, and expiration time (5 minutes)
//	DateTime expirationTime = DateTime.UtcNow.AddMinutes(5);
//	_otpStoreFR[emailKey] = (otp, resetToken, expirationTime);

//	// Send OTP email
//	var emailBody = $@"
//          <p>Hi there,</p>
//          <p>We received a request to reset your password, and we're here to help!</p>
//          <p>Your OTP (One-Time Password) to reset your password is:</p>
//          <h2 style='color: #2E86C1;'>{otp}</h2>
//          <p>This code is valid for the next <strong>5 minutes</strong>.</p>
//          <p>If you didn't request a password reset, no worries—your account is still safe. You can simply ignore this email.</p>
//          <p>Take care,</p>
//          <p><strong>The [Your App Name] Team</strong></p>";

//	await _mailService.SendEmailAsync(model.Email, "Password Reset OTP", emailBody);

//	return Ok("OTP sent to your email.");
//}




//[HttpPost("reset-password")]
//public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO model)
//{
//	if (!ModelState.IsValid)
//		return BadRequest("Invalid request.");

//	// Normalize the email address to lower case to avoid case sensitivity issues
//	string emailKey = model.Email.ToLower();

//	// Check if the OTP exists and has not expired
//	if (!_otpStoreFR.TryGetValue(emailKey, out var otpDetails))
//		return BadRequest("OTP has not been requested or has expired.");

//	var (storedOtp, resetToken, expirationTime) = otpDetails;

//	// Validate OTP and expiration
//	if (DateTime.UtcNow > expirationTime)
//	{
//		_otpStoreFR.Remove(emailKey); // Remove expired OTP
//		return BadRequest("OTP has expired. Please request a new one.");
//	}

//	// Validate the OTP entered by the user
//	if (storedOtp != model.Otp)
//		return BadRequest("Invalid OTP.");

//	// Retrieve the user by email
//	var user = await _userManager.FindByEmailAsync(model.Email);
//	if (user == null)
//		return BadRequest("User not found.");

//	// Reset the password
//	var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);
//	if (result.Succeeded)
//	{
//		_otpStoreFR.Remove(emailKey); // Remove the OTP after successful reset
//		return Ok("Password has been reset successfully.");
//	}

//	// If password reset fails, return error messages
//	foreach (var error in result.Errors)
//	{
//		ModelState.AddModelError(string.Empty, error.Description);
//	}

//	return BadRequest(ModelState);
//}

#endregion
#region Comment
//public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDTO dto)
//{
//	if (!_otpStore.ContainsKey(dto.Email))
//	{
//		return BadRequest("No OTP request found for this email.");
//	}

//	// Compare the OTP entered by the user with the one stored
//	if (_otpStore[dto.Email] != dto.Otp)
//	{
//		return BadRequest("Invalid OTP.");
//	}

//	// Optionally, you can remove the OTP after it is verified
//	_otpStore.Remove(dto.Email);

//	return Ok("OTP verified successfully.");
//} 

#endregion















