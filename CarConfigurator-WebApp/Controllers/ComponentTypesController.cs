using CarConfigurator_WebApp.ViewModels;
using DAL.Models;
using DAL.Services.ComponentTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarConfigurator_WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ComponentTypesController : Controller
    {
        private readonly IComponentTypeService _componentTypeService;
        private readonly IConfiguration _configuration;

        public ComponentTypesController(IComponentTypeService componentTypeService, IConfiguration configuration)
        {
            _componentTypeService = componentTypeService;
            _configuration = configuration;
        }

        // INDEX (Search + Paging)
        [HttpGet]
        public IActionResult Index(string? q = null, int page = 1)
        {
            const int pageSize = 10;
            if (page < 1) page = 1;

            var expandPages = _configuration.GetValue<int?>("Paging:ExpandPages") ?? 5;

            var vm = new ComponentTypesIndexVM
            {
                Query = q,
                Page = page,
                PageSize = pageSize,
                ExpandPages = expandPages,
                TotalCount = _componentTypeService.Count(q)
            };

            var results = _componentTypeService.Search(q, vm.Page, vm.PageSize)
                .OrderBy(t => t.DisplayOrder ?? int.MaxValue)
                .ThenBy(t => t.Name)
                .ToList();

            vm.Items = results.Select(t => new ComponentTypeListItemVM
            {
                Id = t.Id,
                Name = t.Name,
                MinSelect = t.MinSelect,
                MaxSelect = t.MaxSelect,
                DisplayOrder = t.DisplayOrder,
                IsActive = t.IsActive
            }).ToList();

            return View(vm);
        }

        // CREATE (GET)
        [HttpGet]
        public IActionResult Create()
        {
            return View(new ComponentTypeCreateVM { IsActive = true });
        }

        // CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ComponentTypeCreateVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            if (vm.MaxSelect < vm.MinSelect)
            {
                ModelState.AddModelError(nameof(vm.MaxSelect), "MaxSelect mora biti >= MinSelect.");
                return View(vm);
            }

            try
            {
                var entity = new ComponentType
                {
                    Name = vm.Name.Trim(),
                    MinSelect = vm.MinSelect,
                    MaxSelect = vm.MaxSelect,
                    DisplayOrder = vm.DisplayOrder,
                    IsActive = vm.IsActive
                };

                _componentTypeService.Add(entity);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Došlo je do greške prilikom kreiranja tipa komponente.");
                return View(vm);
            }
        }

        // EDIT (GET)
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var entity = _componentTypeService.GetById(id);
            if (entity == null)
                return NotFound();

            var vm = new ComponentTypeEditVM
            {
                Id = entity.Id,
                Name = entity.Name,
                MinSelect = entity.MinSelect,
                MaxSelect = entity.MaxSelect,
                DisplayOrder = entity.DisplayOrder,
                IsActive = entity.IsActive
            };

            return View(vm);
        }

        // EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ComponentTypeEditVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            if (vm.MaxSelect < vm.MinSelect)
            {
                ModelState.AddModelError(nameof(vm.MaxSelect), "MaxSelect mora biti >= MinSelect.");
                return View(vm);
            }

            try
            {
                var existing = _componentTypeService.GetById(vm.Id);
                if (existing == null)
                    return NotFound();

                existing.Name = vm.Name.Trim();
                existing.MinSelect = vm.MinSelect;
                existing.MaxSelect = vm.MaxSelect;
                existing.DisplayOrder = vm.DisplayOrder;
                existing.IsActive = vm.IsActive;

                _componentTypeService.Update(existing);
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
            var entity = _componentTypeService.GetById(id);
            if (entity == null)
                return NotFound();

            var vm = new ComponentTypeDeleteVM
            {
                Id = entity.Id,
                Name = entity.Name,
                MinSelect = entity.MinSelect,
                MaxSelect = entity.MaxSelect,
                DisplayOrder = entity.DisplayOrder,
                IsActive = entity.IsActive
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
                _componentTypeService.Delete(id);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Delete), new { id });
            }
            catch
            {
                TempData["Error"] = "Došlo je do greške prilikom brisanja tipa komponente.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }
    }
}
