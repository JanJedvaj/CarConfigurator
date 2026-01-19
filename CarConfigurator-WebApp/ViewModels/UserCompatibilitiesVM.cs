using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace CarConfigurator_WebApp.ViewModels
{
    public class UserCompatibilitiesVM
    {
        public int? SelectedComponentId { get; set; }
        public List<SelectListItem> Components { get; set; } = new();

        public List<UserCompatibilityItemVM> Allowed { get; set; } = new();
        public List<UserCompatibilityItemVM> Blocked { get; set; } = new();
    }
}
