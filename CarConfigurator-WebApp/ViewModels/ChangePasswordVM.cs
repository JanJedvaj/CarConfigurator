using System.ComponentModel.DataAnnotations;

namespace CarConfigurator_WebApp.ViewModels
{
    public class ChangePasswordVM
    {
        [Required(ErrorMessage = "Stara lozinka je obavezna.")]
        [DataType(DataType.Password)]
        [Display(Name = "Old password")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nova lozinka je obavezna.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Nova lozinka mora imati barem 8 znakova.")]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Potvrda nove lozinke je obavezna.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare(nameof(NewPassword), ErrorMessage = "Nove lozinke se ne podudaraju.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
