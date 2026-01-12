using CarConfigurator_WebApp.ViewModels;
using DAL.Services.Compatibilities;
using DAL.Services.Components;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarConfigurator_WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CompatibilitiesController : Controller
    {
        private readonly ICompatibilityService _compatibilityService;
        private readonly IComponentService _componentService;

        public CompatibilitiesController(
            ICompatibilityService compatibilityService,
            IComponentService componentService)
        {
            _compatibilityService = compatibilityService;
            _componentService = componentService;
        }

        // INDEX
        [HttpGet]
        public IActionResult Index()
        {
            var components = _componentService.GetAll(onlyActive: false).ToList();
            var lookup = components.ToDictionary(c => c.Id, c => c.Title);

            var vm = new CompatibilitiesIndexVM
            {
                Create = new CompatibilityCreateVM
                {
                    Components = components
                        .OrderBy(c => c.Title)
                        .Select(c => new SelectListItem
                        {
                            Value = c.Id.ToString(),
                            Text = c.Title
                        })
                        .ToList()
                }
            };

            var rules = _compatibilityService.GetAll();

            vm.Items = rules.Select(r => new CompatibilityListItemVM
            {
                ComponentId = r.ComponentId,
                ComponentName = lookup.TryGetValue(r.ComponentId, out var a) ? a : "(n/a)",
                CompatibleWithComponentId = r.CompatibleWithComponentId,
                CompatibleWithComponentName = lookup.TryGetValue(r.CompatibleWithComponentId, out var b) ? b : "(n/a)",
                IsAllowed = r.IsAllowed
            }).ToList();

            return View(vm);
        }

        // CREATE / UPDATE RULE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CompatibilityCreateVM vm)
        {
            var components = _componentService.GetAll(onlyActive: false).ToList();

            vm.Components = components
                .OrderBy(c => c.Title)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Title
                })
                .ToList();

            if (!ModelState.IsValid)
            {
                return View("Index", new CompatibilitiesIndexVM
                {
                    Create = vm,
                    Items = BuildList()
                });
            }

            try
            {
                _compatibilityService.SetRule(
                    vm.ComponentId,
                    vm.CompatibleWithComponentId,
                    vm.IsAllowed);

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return View("Index", new CompatibilitiesIndexVM
                {
                    Create = vm,
                    Items = BuildList()
                });
            }
        }

        // DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int componentId, int compatibleWithComponentId)
        {
            _compatibilityService.DeleteRule(componentId, compatibleWithComponentId);
            return RedirectToAction(nameof(Index));
        }

        // helper
        private List<CompatibilityListItemVM> BuildList()
        {
            var components = _componentService.GetAll(onlyActive: false).ToList();
            var lookup = components.ToDictionary(c => c.Id, c => c.Title);

            return _compatibilityService.GetAll()
                .Select(r => new CompatibilityListItemVM
                {
                    ComponentId = r.ComponentId,
                    ComponentName = lookup.TryGetValue(r.ComponentId, out var a) ? a : "(n/a)",
                    CompatibleWithComponentId = r.CompatibleWithComponentId,
                    CompatibleWithComponentName = lookup.TryGetValue(r.CompatibleWithComponentId, out var b) ? b : "(n/a)",
                    IsAllowed = r.IsAllowed
                }).ToList();
        }
    }
}
