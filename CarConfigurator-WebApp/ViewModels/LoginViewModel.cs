using System.ComponentModel.DataAnnotations;

namespace CarConfigurator_WebApp.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Korisničko ime ili e-mail je obavezan.")]
        [Display(Name = "Korisničko ime ili e-mail")]
        public string UserNameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lozinka je obavezna.")]
        [DataType(DataType.Password)]
        [Display(Name = "Lozinka")]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}
