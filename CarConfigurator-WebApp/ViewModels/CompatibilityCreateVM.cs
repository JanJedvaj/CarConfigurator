using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CarConfigurator_WebApp.ViewModels
{
    public class CompatibilityCreateVM
    {
        [Required]
        [Display(Name = "Component")]
        public int ComponentId { get; set; }

        [Required]
        [Display(Name = "Compatible With")]
        public int CompatibleWithComponentId { get; set; }

        [Display(Name = "Allowed")]
        public bool IsAllowed { get; set; } = true;

        public List<SelectListItem> Components { get; set; } = new();
    }
}
