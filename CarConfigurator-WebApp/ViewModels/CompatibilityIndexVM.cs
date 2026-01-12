namespace CarConfigurator_WebApp.ViewModels
{
    public class CompatibilitiesIndexVM
    {
        public CompatibilityCreateVM Create { get; set; } = new();

        public List<CompatibilityListItemVM> Items { get; set; } = new();
    }
}
