using System.ComponentModel.DataAnnotations;

namespace CarConfigurator_WebApp.ViewModels
{
    public class AdminProfileVM
    {
        public int Id { get; set; }

        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Email nije ispravan.")]
        [StringLength(255)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "First name")]
        public string? FirstName { get; set; }

        [StringLength(100)]
        [Display(Name = "Last name")]
        public string? LastName { get; set; }

        [StringLength(50)]
        [Display(Name = "Phone")]
        public string? Phone { get; set; }
    }
}
