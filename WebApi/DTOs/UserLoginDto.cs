using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs.Auth
{
    public class UserLoginDto
    {
        [Required(ErrorMessage = "Username or email is required.")]
        public string UserNameOrEmail { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = null!;
    }
}
