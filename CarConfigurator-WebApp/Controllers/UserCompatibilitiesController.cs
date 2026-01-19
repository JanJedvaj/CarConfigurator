using CarConfigurator_WebApp.ViewModels;
using DAL.Services.Compatibilities;
using DAL.Services.Components;
using DAL.Services.ComponentTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarConfigurator_WebApp.Controllers
{
    [Authorize]
    public class UserCompatibilitiesController : Controller
    {
        private readonly ICompatibilityService _compatibilityService;
        private readonly IComponentService _componentService;
        private readonly IComponentTypeService _componentTypeService;

        public UserCompatibilitiesController(
            ICompatibilityService compatibilityService,
            IComponentService componentService,
            IComponentTypeService componentTypeService)
        {
            _compatibilityService = compatibilityService;
            _componentService = componentService;
            _componentTypeService = componentTypeService;
        }

        [HttpGet]
        public IActionResult Index(int? componentId = null)
        {
            // Aktivne komponente za dropdown
            var components = _componentService.GetAll(onlyActive: true)
                .OrderBy(c => c.Title)
                .ToList();

            var typeLookup = _componentTypeService.GetAll()
                .ToDictionary(t => t.Id, t => t.Name);

            var vm = new UserCompatibilitiesVM
            {
                SelectedComponentId = componentId,
                Components = components.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Title,
                    Selected = componentId.HasValue && componentId.Value == c.Id
                }).ToList()
            };

            if (!componentId.HasValue)
                return View(vm);

            // Allowed lista iz servisa 
            var allowedIds = _compatibilityService
                .GetAllowedCompatibleComponentIds(componentId.Value)
                .ToHashSet();

            var allRules = _compatibilityService.GetAll()
                .Where(r => r.ComponentId == componentId.Value)
                .ToList();

            var blockedIds = allRules
                .Where(r => r.IsAllowed == false)
                .Select(r => r.CompatibleWithComponentId)
                .ToHashSet();

            // Mapiranje allowed/blocked u prikazne iteme
            foreach (var c in components)
            {
                if (c.Id == componentId.Value) continue;

                if (allowedIds.Contains(c.Id))
                {
                    vm.Allowed.Add(new UserCompatibilityItemVM
                    {
                        ComponentId = c.Id,
                        Title = c.Title,
                        Price = c.Price,
                        ComponentTypeName = typeLookup.TryGetValue(c.ComponentTypeId, out var tn) ? tn : ""
                    });
                }
                else if (blockedIds.Contains(c.Id))
                {
                    vm.Blocked.Add(new UserCompatibilityItemVM
                    {
                        ComponentId = c.Id,
                        Title = c.Title,
                        Price = c.Price,
                        ComponentTypeName = typeLookup.TryGetValue(c.ComponentTypeId, out var tn) ? tn : ""
                    });
                }
            }

            vm.Allowed = vm.Allowed.OrderBy(x => x.ComponentTypeName).ThenBy(x => x.Title).ToList();
            vm.Blocked = vm.Blocked.OrderBy(x => x.ComponentTypeName).ThenBy(x => x.Title).ToList();

            return View(vm);
        }
    }
}
