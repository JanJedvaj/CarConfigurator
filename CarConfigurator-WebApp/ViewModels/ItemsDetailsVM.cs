namespace CarConfigurator_WebApp.ViewModels
{
    public class ItemDetailsVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string ComponentTypeName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }

    }
}
