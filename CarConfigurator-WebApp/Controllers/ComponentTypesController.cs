using AutoMapper;
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
        private readonly IMapper _mapper;

        public ComponentTypesController(IComponentTypeService componentTypeService, IConfiguration configuration, IMapper mapper)
        {
            _componentTypeService = componentTypeService;
            _configuration = configuration;
            _mapper = mapper;
        }

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

            vm.Items = _mapper.Map<List<ComponentTypeListItemVM>>(results);
            return View(vm);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new ComponentTypeCreateVM { IsActive = true });
        }

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
                vm.Name = vm.Name.Trim();

                var entity = _mapper.Map<ComponentType>(vm);
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

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var entity = _componentTypeService.GetById(id);
            if (entity == null) return NotFound();

            var vm = _mapper.Map<ComponentTypeEditVM>(entity);
            return View(vm);
        }

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
                if (existing == null) return NotFound();

                vm.Name = vm.Name.Trim();

                _mapper.Map(vm, existing);
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

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var entity = _componentTypeService.GetById(id);
            if (entity == null) return NotFound();

            var vm = _mapper.Map<ComponentTypeDeleteVM>(entity);
            return View(vm);
        }

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
