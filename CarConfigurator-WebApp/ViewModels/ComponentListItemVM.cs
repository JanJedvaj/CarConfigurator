namespace CarConfigurator_WebApp.ViewModels
{
    public class ComponentListItemVM
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ComponentTypeName { get; set; } = string.Empty;

        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }
}
