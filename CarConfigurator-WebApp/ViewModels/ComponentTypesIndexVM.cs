namespace CarConfigurator_WebApp.ViewModels
{
    public class ComponentTypesIndexVM
    {
        public string? Query { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public int TotalCount { get; set; }
        public int TotalPages =>
            PageSize <= 0 ? 1 : (int)Math.Ceiling((double)TotalCount / PageSize);

        public int ExpandPages { get; set; } = 5;

        public List<ComponentTypeListItemVM> Items { get; set; } = new();

        public int FirstPageToShow
        {
            get
            {
                var from = Page - ExpandPages;
                return from < 1 ? 1 : from;
            }
        }

        public int LastPageToShow
        {
            get
            {
                var to = Page + ExpandPages;
                return to > TotalPages ? TotalPages : to;
            }
        }
    }
}
