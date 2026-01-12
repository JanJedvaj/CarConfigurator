using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarConfigurator_WebApp.ViewModels
{
    public class MyConfigurationVM
    {
        public int ConfigurationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }

        public List<ConfigurationComponentItemVM> Items { get; set; } = new();

        // Choose existing configuration
        public int SelectedConfigurationId { get; set; }
        public List<SelectListItem> Configurations { get; set; } = new();

        // Create new configuration
        public string? NewConfigurationName { get; set; }
    }
}
