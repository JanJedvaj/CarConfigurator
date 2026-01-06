using System.ComponentModel.DataAnnotations;

namespace CarConfigurator_WebApp.ViewModels
{
    public class ComponentTypeCreateVM
    {
        [Required(ErrorMessage = "Name je obavezan.")]
        [StringLength(100, ErrorMessage = "Name može imati najviše 100 znakova.")]
        [Display(Name = "Name (unique)")]
        public string Name { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "MinSelect mora biti 0 ili veći.")]
        [Display(Name = "Min Select")]
        public int MinSelect { get; set; } = 0;

        [Range(0, 100, ErrorMessage = "MaxSelect mora biti 0 ili veći.")]
        [Display(Name = "Max Select")]
        public int MaxSelect { get; set; } = 1;

        [Display(Name = "Display Order")]
        public int? DisplayOrder { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
