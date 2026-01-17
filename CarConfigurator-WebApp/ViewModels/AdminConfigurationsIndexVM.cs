using System.Collections.Generic;

namespace CarConfigurator_WebApp.ViewModels
{
    public class AdminConfigurationsIndexVM
    {
        public string? Query { get; set; }
        public List<AdminConfigurationListItemVM> Items { get; set; } = new();
    }
}
