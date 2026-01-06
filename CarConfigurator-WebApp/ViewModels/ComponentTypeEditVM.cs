using System.ComponentModel.DataAnnotations;

namespace CarConfigurator_WebApp.ViewModels
{
    public class ComponentTypeEditVM
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name je obavezan.")]
        [StringLength(100, ErrorMessage = "Name može imati najviše 100 znakova.")]
        [Display(Name = "Name (unique)")]
        public string Name { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "MinSelect mora biti 0 ili veći.")]
        [Display(Name = "Min Select")]
        public int MinSelect { get; set; }

        [Range(0, 100, ErrorMessage = "MaxSelect mora biti 0 ili veći.")]
        [Display(Name = "Max Select")]
        public int MaxSelect { get; set; }

        [Display(Name = "Display Order")]
        public int? DisplayOrder { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }
}
