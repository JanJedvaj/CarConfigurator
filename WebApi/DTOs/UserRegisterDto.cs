using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs.Auth
{
    public class UserRegisterDto
    {
        [Required(ErrorMessage = "User name is required.")]
        [StringLength(100)]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Provide a correct e-mail address.")]
        [StringLength(255)]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(256, MinimumLength = 8, ErrorMessage = "Password should be at least 8 characters long.")]
        public string Password { get; set; } = null!;

        // Admin može birati, ali za običan register default "User"
        public string? Role { get; set; }

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [StringLength(50)]
        public string? Phone { get; set; }
    }
}
