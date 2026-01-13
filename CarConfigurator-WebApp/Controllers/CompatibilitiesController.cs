using AutoMapper;
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
        private readonly IMapper _mapper;

        public CompatibilitiesController(
            ICompatibilityService compatibilityService,
            IComponentService componentService,
            IMapper mapper)
        {
            _compatibilityService = compatibilityService;
            _componentService = componentService;
            _mapper = mapper;
        }

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
                        .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Title })
                        .ToList()
                }
            };

            var rules = _compatibilityService.GetAll().ToList();
            vm.Items = _mapper.Map<List<CompatibilityListItemVM>>(rules);

            foreach (var item in vm.Items)
            {
                item.ComponentName = lookup.TryGetValue(item.ComponentId, out var a) ? a : "(n/a)";
                item.CompatibleWithComponentName = lookup.TryGetValue(item.CompatibleWithComponentId, out var b) ? b : "(n/a)";
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CompatibilityCreateVM vm)
        {
            var components = _componentService.GetAll(onlyActive: false).ToList();
            vm.Components = components
                .OrderBy(c => c.Title)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Title })
                .ToList();

            if (!ModelState.IsValid)
                return View("Index", new CompatibilitiesIndexVM { Create = vm, Items = BuildList() });

            try
            {
                _compatibilityService.SetRule(vm.ComponentId, vm.CompatibleWithComponentId, vm.IsAllowed);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Index", new CompatibilitiesIndexVM { Create = vm, Items = BuildList() });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int componentId, int compatibleWithComponentId)
        {
            _compatibilityService.DeleteRule(componentId, compatibleWithComponentId);
            return RedirectToAction(nameof(Index));
        }

        private List<CompatibilityListItemVM> BuildList()
        {
            var components = _componentService.GetAll(onlyActive: false).ToList();
            var lookup = components.ToDictionary(c => c.Id, c => c.Title);

            var rules = _compatibilityService.GetAll().ToList();
            var list = _mapper.Map<List<CompatibilityListItemVM>>(rules);

            foreach (var item in list)
            {
                item.ComponentName = lookup.TryGetValue(item.ComponentId, out var a) ? a : "(n/a)";
                item.CompatibleWithComponentName = lookup.TryGetValue(item.CompatibleWithComponentId, out var b) ? b : "(n/a)";
            }

            return list;
        }
    }
}
