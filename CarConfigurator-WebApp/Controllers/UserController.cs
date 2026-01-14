using CarConfigurator_WebApp.ViewModels;
using DAL.Services.ComponentTypes;
using DAL.Services.Configurations;
using DAL.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarConfigurator_WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly IConfigurationService _configurationService;
        private readonly IComponentTypeService _componentTypeService;

        public UsersController(
            IUserService userService,
            IConfigurationService configurationService,
            IComponentTypeService componentTypeService)
        {
            _userService = userService;
            _configurationService = configurationService;
            _componentTypeService = componentTypeService;
        }

        // =========================
        // LIST USERS
        // =========================
        [HttpGet]
        public IActionResult Index(string? q = null)
        {
            var users = _userService.GetAllUsers();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var query = q.Trim();
                users = users.Where(u =>
                    (!string.IsNullOrEmpty(u.UserName) && u.UserName.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(u.Email) && u.Email.Contains(query, StringComparison.OrdinalIgnoreCase)));
            }

            var vm = new UsersIndexVM
            {
                Query = q,
                Items = users
                    .OrderBy(u => u.UserName)
                    .Select(u => new UserListItemVM
                    {
                        Id = u.Id,
                        UserName = u.UserName,
                        Email = u.Email,
                        Role = u.Role,
                        IsActive = u.IsActive,
                        CreatedAt = u.CreatedAt,
                        LastLoginAt = u.LastLoginAt
                    })
                    .ToList()
            };

            return View(vm);
        }

        // =========================
        // USER DETAILS + "actions"
        // =========================
        [HttpGet]
        public IActionResult Details(int id)
        {
            var user = _userService.GetUser(id);
            if (user == null) return NotFound();

            // lookup tipova za prikaz ComponentTypeName (ne oslanjamo se na Include)
            var typeLookup = _componentTypeService.GetAll()
                .ToDictionary(t => t.Id, t => t.Name);

            var configs = _configurationService.GetUserConfigurations(user.Id)
                .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
                .ToList();

            var vm = new UserDetailsVM
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };

            foreach (var c in configs)
            {
                var details = _configurationService.GetConfigurationDetails(c.Id) ?? c;

                var configVm = new UserConfigurationVM
                {
                    ConfigurationId = details.Id,
                    Name = details.Name,
                    CreatedAt = details.CreatedAt,
                    UpdatedAt = details.UpdatedAt,
                    TotalPrice = details.TotalPrice ?? _configurationService.RecalculateTotalPrice(details.Id)
                };

                if (details.CarConfigurationComponents != null)
                {
                    foreach (var cc in details.CarConfigurationComponents)
                    {
                        var comp = cc.Component;
                        if (comp == null) continue;

                        configVm.Components.Add(new UserConfigurationComponentVM
                        {
                            ComponentId = comp.Id,
                            Title = comp.Title,
                            ComponentTypeName = typeLookup.TryGetValue(comp.ComponentTypeId, out var typeName) ? typeName : "",
                            Price = comp.Price
                        });
                    }

                    configVm.Components = configVm.Components
                        .OrderBy(x => x.ComponentTypeName)
                        .ThenBy(x => x.Title)
                        .ToList();
                }

                vm.Configurations.Add(configVm);
            }

            return View(vm);
        }
    }
}
