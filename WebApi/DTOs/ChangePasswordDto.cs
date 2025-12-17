using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs.Auth
{
    public class ChangePasswordDto
    {
        [Required]
        public string UserName { get; set; } = null!;

        [Required]
        public string OldPassword { get; set; } = null!;

        [Required]
        [StringLength(256, MinimumLength = 8, ErrorMessage = "Password should be at least 8 characters long.")]
        public string NewPassword { get; set; } = null!;
    }
}
