using System.ComponentModel.DataAnnotations;

namespace Edu_plat.DTO.ProfileDto
{
    public class PhoneNumberDto
    {
        [RegularExpression(@"^1[0125]\d{8}$", ErrorMessage = "Phone number must start with '1' followed by '0', '1', '2', or '5' and be 10 digits long.")]
        public string NewphoneNumber { get; set; } = string.Empty;
    }
}
