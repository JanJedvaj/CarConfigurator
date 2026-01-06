using CarConfigurator_WebApp.ViewModels;
using DAL.Models;
using DAL.Services.Components;
using DAL.Services.ComponentTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarConfigurator_WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ComponentsController : Controller
    {
        private readonly IComponentService _componentService;
        private readonly IComponentTypeService _componentTypeService;
        private readonly IConfiguration _configuration;

        public ComponentsController(
            IComponentService componentService,
            IComponentTypeService componentTypeService,
            IConfiguration configuration)
        {
            _componentService = componentService;
            _componentTypeService = componentTypeService;
            _configuration = configuration;
        }

        // INDEX (List + Search + Filter + Paging)
        [HttpGet]
        public IActionResult Index(string? q = null, int? componentTypeId = null, int page = 1)
        {
            const int pageSize = 10;
            if (page < 1) page = 1;

            var expandPages = _configuration.GetValue<int?>("Paging:ExpandPages") ?? 5;

            var types = _componentTypeService.GetAll().ToList();
            var typeLookup = types.ToDictionary(t => t.Id, t => t.Name);

            var vm = new ComponentsIndexVM
            {
                Query = q,
                ComponentTypeId = componentTypeId,
                Page = page,
                PageSize = pageSize,
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

            // filter po tipu komponenti
            if (componentTypeId.HasValue)
            {
                var items = _componentService.GetByComponentType(componentTypeId.Value);

                if (!string.IsNullOrWhiteSpace(q))
                {
                    var query = q.Trim();
                    items = items.Where(c =>
                        (!string.IsNullOrEmpty(c.Title) && c.Title.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(c.Name) && c.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(c.Description) && c.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
                    );
                }

                var filtered = items.ToList();
                vm.TotalCount = filtered.Count;

                var pageItems = filtered
                    .OrderBy(c => c.SortOrder ?? int.MaxValue)
                    .ThenBy(c => c.Title)
                    .Skip((vm.Page - 1) * vm.PageSize)
                    .Take(vm.PageSize)
                    .ToList();

                vm.Items = pageItems.Select(c => new ComponentListItemVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    Title = c.Title,
                    Price = c.Price,
                    IsActive = c.IsActive,
                    ComponentTypeName = typeLookup.TryGetValue(c.ComponentTypeId, out var typeName)
                        ? typeName
                        : "(n/a)"
                }).ToList();

                return View(vm);
            }

            // search + paging bez filtera po tipu
            vm.TotalCount = _componentService.Count(q, onlyActive: false);

            var results = _componentService
                .Search(q, vm.Page, vm.PageSize, onlyActive: false)
                .OrderBy(c => c.SortOrder ?? int.MaxValue)
                .ThenBy(c => c.Title)
                .ToList();

            vm.Items = results.Select(c => new ComponentListItemVM
            {
                Id = c.Id,
                Name = c.Name,
                Title = c.Title,
                Price = c.Price,
                IsActive = c.IsActive,
                ComponentTypeName = typeLookup.TryGetValue(c.ComponentTypeId, out var typeName)
                    ? typeName
                    : "(n/a)"
            }).ToList();

            return View(vm);
        }

        // CREATE (GET)
        [HttpGet]
        public IActionResult Create()
        {
            var types = _componentTypeService.GetAll().ToList();

            var vm = new ComponentCreateVM
            {
                IsActive = true,
                ComponentTypes = types
                    .OrderBy(t => t.DisplayOrder ?? int.MaxValue)
                    .ThenBy(t => t.Name)
                    .Select(t => new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = t.Name
                    })
                    .ToList()
            };

            return View(vm);
        }

        // CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ComponentCreateVM vm)
        {
            var types = _componentTypeService.GetAll().ToList();
            vm.ComponentTypes = types
                .OrderBy(t => t.DisplayOrder ?? int.MaxValue)
                .ThenBy(t => t.Name)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name,
                    Selected = t.Id == vm.ComponentTypeId
                })
                .ToList();

            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var entity = new Component
                {
                    Name = vm.Name.Trim(),
                    Title = vm.Title.Trim(),
                    Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim(),
                    Price = vm.Price,
                    IsActive = vm.IsActive,
                    SortOrder = vm.SortOrder,
                    ComponentTypeId = vm.ComponentTypeId,
                    ImageId = vm.ImageId,
                    CreatedAt = DateTime.UtcNow
                };

                _componentService.Create(entity);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Došlo je do greške prilikom kreiranja komponente.");
                return View(vm);
            }
        }

        // EDIT (GET)
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var entity = _componentService.GetById(id);
            if (entity == null)
                return NotFound();

            var types = _componentTypeService.GetAll().ToList();

            var vm = new ComponentEditVM
            {
                Id = entity.Id,
                Name = entity.Name,
                Title = entity.Title,
                Description = entity.Description,
                Price = entity.Price,
                IsActive = entity.IsActive,
                SortOrder = entity.SortOrder,
                ComponentTypeId = entity.ComponentTypeId,
                ImageId = entity.ImageId,
                ComponentTypes = types
                    .OrderBy(t => t.DisplayOrder ?? int.MaxValue)
                    .ThenBy(t => t.Name)
                    .Select(t => new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = t.Name,
                        Selected = t.Id == entity.ComponentTypeId
                    })
                    .ToList()
            };

            return View(vm);
        }

        // EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ComponentEditVM vm)
        {
            var types = _componentTypeService.GetAll().ToList();
            vm.ComponentTypes = types
                .OrderBy(t => t.DisplayOrder ?? int.MaxValue)
                .ThenBy(t => t.Name)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name,
                    Selected = t.Id == vm.ComponentTypeId
                })
                .ToList();

            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var existing = _componentService.GetById(vm.Id);
                if (existing == null)
                    return NotFound();

                existing.Name = vm.Name.Trim();
                existing.Title = vm.Title.Trim();
                existing.Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim();
                existing.Price = vm.Price;
                existing.IsActive = vm.IsActive;
                existing.SortOrder = vm.SortOrder;
                existing.ComponentTypeId = vm.ComponentTypeId;
                existing.ImageId = vm.ImageId;

                _componentService.Update(existing);

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Došlo je do greške prilikom spremanja promjena.");
                return View(vm);
            }
        }

        // DELETE (GET)
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var entity = _componentService.GetById(id);
            if (entity == null)
                return NotFound();

            var type = _componentTypeService.GetById(entity.ComponentTypeId);

            var vm = new ComponentDeleteVM
            {
                Id = entity.Id,
                Name = entity.Name,
                Title = entity.Title,
                Price = entity.Price,
                IsActive = entity.IsActive,
                ComponentTypeName = type?.Name ?? "(n/a)"
            };

            return View(vm);
        }

        // DELETE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                _componentService.Delete(id);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Delete), new { id });
            }
            catch
            {
                TempData["Error"] = "Došlo je do greške prilikom brisanja komponente.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }
    }
}
