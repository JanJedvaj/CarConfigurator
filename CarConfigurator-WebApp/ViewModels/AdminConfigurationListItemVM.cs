using System;

namespace CarConfigurator_WebApp.ViewModels
{
    public class AdminConfigurationListItemVM
    {
        public int Id { get; set; } 
        public string ConfigurationName { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Status { get; set; } = "";
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
