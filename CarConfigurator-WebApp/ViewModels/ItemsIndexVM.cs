using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarConfigurator_WebApp.ViewModels
{
    public class ItemsIndexVM
    {
        public string? Query { get; set; }
        public int? ComponentTypeId { get; set; }

        public List<SelectListItem> ComponentTypes { get; set; } = new();
        public List<ItemsListItemVM> Items { get; set; } = new();

        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public bool HasPrevious => Page > 1;
        public bool HasNext => Page * PageSize < TotalCount;
    }
}
