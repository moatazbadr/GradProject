using System.ComponentModel.DataAnnotations;

namespace Edu_plat.DTO.ProfileDto
{
    public class ChangepasswordDto
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
