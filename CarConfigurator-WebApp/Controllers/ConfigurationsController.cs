using AutoMapper;
using CarConfigurator_WebApp.ViewModels;
using DAL.Services.Configurations;
using DAL.Services.Components;
using DAL.Services.ComponentTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace CarConfigurator_WebApp.Controllers
{
    [Authorize]
    public class ConfigurationsController : Controller
    {
        private readonly IConfigurationService _configurationService;
        private readonly IComponentTypeService _componentTypeService;
        private readonly IComponentService _componentService;
        private readonly IMapper _mapper;

        public ConfigurationsController(
            IConfigurationService configurationService,
            IComponentService componentService,
            IMapper mapper,
            IComponentTypeService componentTypeService)
        {
            _configurationService = configurationService;
            _componentService = componentService;
            _mapper = mapper;
            _componentTypeService = componentTypeService;
        }

        [HttpGet]
        public IActionResult My(int? configurationId = null)
        {
            var userId = GetUserIdOrThrow();

            var configs = _configurationService.GetUserConfigurations(userId).ToList();
            if (configs.Count == 0)
            {
                var newId = _configurationService.CreateConfiguration(userId, "My Configuration");
                return RedirectToAction(nameof(My), new { configurationId = newId });
            }

            var selected = configurationId.HasValue
                ? configs.FirstOrDefault(c => c.Id == configurationId.Value)
                : configs.OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt).First();

            if (selected == null)
                selected = configs.OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt).First();

            var details = _configurationService.GetConfigurationDetails(selected.Id)
                          ?? _configurationService.GetConfiguration(selected.Id)
                          ?? selected;

            var dropdown = configs
                .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = c.Id == selected!.Id
                }).ToList();

            var vm = new MyConfigurationVM
            {
                ConfigurationId = details.Id,
                SelectedConfigurationId = details.Id,
                Configurations = dropdown,
                Name = details.Name,
                TotalPrice = details.TotalPrice ?? _configurationService.RecalculateTotalPrice(details.Id)
            };

            var typeLookup = _componentTypeService
                .GetAll()
                .ToDictionary(t => t.Id, t => t.Name);

            if (details.CarConfigurationComponents != null)
            {
                foreach (var cc in details.CarConfigurationComponents)
                {
                    var comp = cc.Component ?? _componentService.GetById(cc.ComponentId);
                    if (comp == null) continue;

                    var item = _mapper.Map<ConfigurationComponentItemVM>(comp);

                    item.ComponentTypeName = typeLookup.TryGetValue(comp.ComponentTypeId, out var typeName)
                        ? typeName
                        : "";

                    vm.Items.Add(item);
                }

                vm.Items = vm.Items
                    .OrderBy(i => i.ComponentTypeName)
                    .ThenBy(i => i.Title)
                    .ToList();
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateNew(string? name)
        {
            var userId = GetUserIdOrThrow();
            var finalName = string.IsNullOrWhiteSpace(name) ? "My Configuration" : name.Trim();
            var id = _configurationService.CreateConfiguration(userId, finalName);
            return RedirectToAction(nameof(My), new { configurationId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddComponent(int componentId, int? configurationId = null)
        {
            var userId = GetUserIdOrThrow();

            var component = _componentService.GetById(componentId);
            if (component == null || !component.IsActive)
                return NotFound();

            var configs = _configurationService.GetUserConfigurations(userId).ToList();

            int targetConfigId;
            if (configurationId.HasValue && configs.Any(c => c.Id == configurationId.Value))
                targetConfigId = configurationId.Value;
            else
                targetConfigId = configs.OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt).First().Id;

            try
            {
                _configurationService.AddComponentToConfiguration(targetConfigId, componentId);
                return RedirectToAction(nameof(My), new { configurationId = targetConfigId });
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Details", "Items", new { id = componentId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveComponent(int configurationId, int componentId)
        {
            var userId = GetUserIdOrThrow();

            var config = _configurationService.GetConfiguration(configurationId);
            if (config == null) return NotFound();
            if (config.UserId != userId) return Forbid();

            _configurationService.RemoveComponentFromConfiguration(configurationId, componentId);
            return RedirectToAction(nameof(My), new { configurationId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear(int configurationId)
        {
            var userId = GetUserIdOrThrow();

            var config = _configurationService.GetConfiguration(configurationId);
            if (config == null) return NotFound();
            if (config.UserId != userId) return Forbid();

            _configurationService.ClearConfiguration(configurationId);
            return RedirectToAction(nameof(My), new { configurationId });
        }

        private int GetUserIdOrThrow()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(idClaim) || !int.TryParse(idClaim, out var userId))
                throw new InvalidOperationException("User not authenticated properly.");
            return userId;
        }
    }
}
