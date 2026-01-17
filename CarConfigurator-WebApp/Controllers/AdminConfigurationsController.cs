using CarConfigurator_WebApp.ViewModels;
using DAL.Services.Components;
using DAL.Services.ComponentTypes;
using DAL.Services.Configurations;
using DAL.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarConfigurator_WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminConfigurationsController : Controller
    {
        private readonly IUserService _userService;
        private readonly IConfigurationService _configurationService;
        private readonly IComponentService _componentService;
        private readonly IComponentTypeService _componentTypeService;

        public AdminConfigurationsController(
            IUserService userService,
            IConfigurationService configurationService,
            IComponentService componentService,
            IComponentTypeService componentTypeService)
        {
            _userService = userService;
            _configurationService = configurationService;
            _componentService = componentService;
            _componentTypeService = componentTypeService;
        }
        [HttpGet]
        public IActionResult Index(string? q = null)
        {
            var users = _userService.GetAllUsers().ToList();
            var userLookup = users.ToDictionary(u => u.Id, u => u.UserName);

            // Učitam sve konfiguracije tako da ih uzmemo po useru 
            var allConfigs = new List<DAL.Models.CarConfiguration>();
            foreach (var u in users)
            {
                allConfigs.AddRange(_configurationService.GetUserConfigurations(u.Id));
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                var query = q.Trim();
                allConfigs = allConfigs
                    .Where(c =>
                        (!string.IsNullOrEmpty(c.Name) && c.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                        (userLookup.TryGetValue(c.UserId, out var uname) && uname.Contains(query, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            var vm = new AdminConfigurationsIndexVM
            {
                Query = q,
                Items = allConfigs
                    .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
                    .Select(c => new AdminConfigurationListItemVM
                    {
                        Id = c.Id,
                        ConfigurationName = c.Name,
                        UserName = userLookup.TryGetValue(c.UserId, out var uname) ? uname : "(unknown)",
                        Status = c.Status,
                        TotalPrice = c.TotalPrice ?? _configurationService.RecalculateTotalPrice(c.Id),
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    })
                    .ToList()
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var config = _configurationService.GetConfiguration(id);
            if (config == null) return NotFound();

            var user = _userService.GetUser(config.UserId);
            var username = user?.UserName ?? "(unknown)";

            var typeLookup = _componentTypeService.GetAll()
                .ToDictionary(t => t.Id, t => t.Name);

            var details = _configurationService.GetConfigurationDetails(id) ?? config;

            var vm = new AdminConfigurationDetailsVM
            {
                Id = details.Id,
                ConfigurationName = details.Name,
                UserName = username,
                Status = details.Status,
                TotalPrice = details.TotalPrice ?? _configurationService.RecalculateTotalPrice(details.Id),
                CreatedAt = details.CreatedAt,
                UpdatedAt = details.UpdatedAt
            };

            if (details.CarConfigurationComponents != null)
            {
                foreach (var cc in details.CarConfigurationComponents)
                {
                    var comp = cc.Component ?? _componentService.GetById(cc.ComponentId);
                    if (comp == null) continue;

                    vm.Lines.Add(new AdminConfigurationLineItemVM
                    {
                        ComponentTypeName = typeLookup.TryGetValue(comp.ComponentTypeId, out var typeName) ? typeName : "",
                        ComponentTitle = comp.Title,
                        Price = comp.Price
                    });
                }

                vm.Lines = vm.Lines
                    .OrderBy(l => l.ComponentTypeName)
                    .ThenBy(l => l.ComponentTitle)
                    .ToList();
            }

            return View(vm);
        }
    }
}
