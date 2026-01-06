namespace CarConfigurator_WebApp.ViewModels
{
    public class ComponentTypeListItemVM
    {
        public int Id { get; set; } 
        public string Name { get; set; } = string.Empty;

        public int MinSelect { get; set; }
        public int MaxSelect { get; set; }
        public int? DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
