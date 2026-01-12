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

        private const int PageSize = 10;

        public ItemsController(
            IComponentService componentService,
            IComponentTypeService componentTypeService)
        {
            _componentService = componentService;
            _componentTypeService = componentTypeService;
        }

        // INDEX (LIST)
        [HttpGet]
        public IActionResult Index(string? query, int? componentTypeId, int page = 1)
        {
            var types = _componentTypeService.GetForConfigurator().ToList();

            var items = _componentService.Search(
                query,
                page,
                PageSize,
                onlyActive: true);

            if (componentTypeId.HasValue)
            {
                items = items.Where(c => c.ComponentTypeId == componentTypeId.Value);
            }

            var totalCount = _componentService.Count(query, onlyActive: true);

            var vm = new ItemsIndexVM
            {
                Query = query,
                ComponentTypeId = componentTypeId,
                Page = page,
                PageSize = PageSize,
                TotalCount = totalCount,

                ComponentTypes = types.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name,
                    Selected = componentTypeId == t.Id
                }).ToList(),

                Items = items.Select(c => new ItemsListItemVM
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Price = c.Price,
                    ComponentTypeName = c.ComponentType.Name
                }).ToList()
            };

            return View(vm);
        }

        // DETAILS
        [HttpGet]
        public IActionResult Details(int id)
        {
            var component = _componentService.GetById(id);
            if (component == null || !component.IsActive)
                return NotFound();

            var vm = new ItemDetailsVM
            {
                Id = component.Id,
                Title = component.Title,
                Description = component.Description,
                Price = component.Price,
                ComponentTypeName = component.ComponentType.Name
            };

            return View(vm);
        }
    }
}
