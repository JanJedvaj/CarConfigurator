using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CarConfigurator_WebApp.ViewModels
{
    public class ComponentEditVM
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name je obavezan.")]
        [StringLength(150, ErrorMessage = "Name može imati najviše 150 znakova.")]
        [Display(Name = "Name (unique)")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Title je obavezan.")]
        [StringLength(200, ErrorMessage = "Title može imati najviše 200 znakova.")]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description može imati najviše 1000 znakova.")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Range(0, 999999999, ErrorMessage = "Price mora biti 0 ili veći.")]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Sort Order")]
        public int? SortOrder { get; set; }

        [Required(ErrorMessage = "Component type je obavezan.")]
        [Display(Name = "Component Type")]
        public int ComponentTypeId { get; set; }

        [Display(Name = "Image")]
        public int? ImageId { get; set; }

        public List<SelectListItem> ComponentTypes { get; set; } = new();
    }
}
