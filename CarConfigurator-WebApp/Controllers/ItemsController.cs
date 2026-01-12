using CarConfigurator_WebApp.ViewModels;
using DAL.Services.Components;
using DAL.Services.ComponentTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarConfigurator_WebApp.Controllers
{
    [Authorize]
    public class ItemsController : Controller
    {
        private readonly IComponentService _componentService;
        private readonly IComponentTypeService _componentTypeService;
        private readonly IConfiguration _configuration;

        private const int PageSize = 10;

        public ItemsController(
            IComponentService componentService,
            IComponentTypeService componentTypeService,
            IConfiguration configuration)
        {
            _componentService = componentService;
            _componentTypeService = componentTypeService;
            _configuration = configuration;
        }

        // INDEX 
        [HttpGet]
        public IActionResult Index(string? query, int? componentTypeId, int page = 1)
        {
            var vm = BuildItemsVm(query, componentTypeId, page);
            return View(vm);
        }

        // AJAX PARTIAL (list + paging)
        [HttpGet]
        public IActionResult ListPartial(string? query, int? componentTypeId, int page = 1)
        {
            var vm = BuildItemsVm(query, componentTypeId, page);
            return PartialView("_ItemsTablePartial", vm);
        }

        // DETAILS
        [HttpGet]
        public IActionResult Details(int id)
        {
            var component = _componentService.GetById(id);
            if (component == null || !component.IsActive)
                return NotFound();

            // lookup type name (sigurno)
            var types = _componentTypeService.GetForConfigurator().ToList();
            var typeLookup = types.ToDictionary(t => t.Id, t => t.Name);

            var vm = new ItemDetailsVM
            {
                Id = component.Id,
                Title = component.Title,
                Description = component.Description,
                Price = component.Price,
                ComponentTypeName = typeLookup.TryGetValue(component.ComponentTypeId, out var tName) ? tName : ""
            };

            return View(vm);
        }

        // helper: build VM with correct counts
        private ItemsIndexVM BuildItemsVm(string? query, int? componentTypeId, int page)
        {
            if (page < 1) page = 1;

            var expandPages = _configuration.GetValue<int?>("Paging:ExpandPages") ?? 5;

            var types = _componentTypeService.GetForConfigurator().ToList();
            var typeLookup = types.ToDictionary(t => t.Id, t => t.Name);

            var vm = new ItemsIndexVM
            {
                Query = query,
                ComponentTypeId = componentTypeId,
                Page = page,
                PageSize = PageSize,
                ExpandPages = expandPages,
                ComponentTypes = types
                    .OrderBy(t => t.DisplayOrder ?? int.MaxValue)
                    .ThenBy(t => t.Name)
                    .Select(t => new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = t.Name,
                        Selected = componentTypeId.HasValue && componentTypeId.Value == t.Id
                    })
                    .ToList()
            };

            // Ako postoji filter po tipu, count/paging na filtriranom skupu (da count bude točan)
            if (componentTypeId.HasValue)
            {
                var items = _componentService.GetByComponentType(componentTypeId.Value)
                    .Where(c => c.IsActive);

                if (!string.IsNullOrWhiteSpace(query))
                {
                    var q = query.Trim();
                    items = items.Where(c =>
                        (!string.IsNullOrEmpty(c.Title) && c.Title.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(c.Name) && c.Name.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(c.Description) && c.Description.Contains(q, StringComparison.OrdinalIgnoreCase)));
                }

                var filtered = items.ToList();
                vm.TotalCount = filtered.Count;

                var pageItems = filtered
                    .OrderBy(c => c.SortOrder ?? int.MaxValue)
                    .ThenBy(c => c.Title)
                    .Skip((vm.Page - 1) * vm.PageSize)
                    .Take(vm.PageSize)
                    .ToList();

                vm.Items = pageItems.Select(c => new ItemsListItemVM
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Price = c.Price,
                    ComponentTypeName = typeLookup.TryGetValue(c.ComponentTypeId, out var tName) ? tName : ""
                }).ToList();

                return vm;
            }

            // Bez filtera po tipu: Search/Count iz servisa
            vm.TotalCount = _componentService.Count(query, onlyActive: true);

            var results = _componentService.Search(query, vm.Page, vm.PageSize, onlyActive: true)
                .OrderBy(c => c.SortOrder ?? int.MaxValue)
                .ThenBy(c => c.Title)
                .ToList();

            vm.Items = results.Select(c => new ItemsListItemVM
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Price = c.Price,
                ComponentTypeName = typeLookup.TryGetValue(c.ComponentTypeId, out var tName) ? tName : ""
            }).ToList();

            return vm;
        }
    }
}
