namespace CarConfigurator_WebApp.ViewModels
{
    public class ConfigurationComponentItemVM
    {
        public int ComponentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ComponentTypeName { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
