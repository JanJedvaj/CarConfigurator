namespace CarConfigurator_WebApp.ViewModels
{
    public class CompatibilityListItemVM
    {
        public int ComponentId { get; set; }
        public string ComponentName { get; set; } = string.Empty;

        public int CompatibleWithComponentId { get; set; }
        public string CompatibleWithComponentName { get; set; } = string.Empty;

        public bool IsAllowed { get; set; }
    }
}
