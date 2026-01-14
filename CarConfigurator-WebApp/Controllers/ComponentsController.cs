using AutoMapper;
using CarConfigurator_WebApp.ViewModels;
using DAL.Models;
using DAL.Services.Components;
using DAL.Services.ComponentTypes;
using DAL.Services.Images;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarConfigurator_WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ComponentsController : Controller
    {
        private readonly IComponentService _componentService;
        private readonly IImageService _imageService;
        private readonly IComponentTypeService _componentTypeService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public ComponentsController(
            IComponentService componentService,
            IImageService imageService,
            IComponentTypeService componentTypeService,
            IConfiguration configuration,
            IMapper mapper)
        {
            _componentService = componentService;
            _imageService = imageService;
            _componentTypeService = componentTypeService;
            _configuration = configuration;
            _mapper = mapper;
        }

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

            if (componentTypeId.HasValue)
            {
                var items = _componentService.GetByComponentType(componentTypeId.Value);

                if (!string.IsNullOrWhiteSpace(q))
                {
                    var query = q.Trim();
                    items = items.Where(c =>
                        (!string.IsNullOrEmpty(c.Title) && c.Title.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(c.Name) && c.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(c.Description) && c.Description.Contains(query, StringComparison.OrdinalIgnoreCase)));
                }

                var filtered = items.ToList();
                vm.TotalCount = filtered.Count;

                var pageItems = filtered
                    .OrderBy(c => c.SortOrder ?? int.MaxValue)
                    .ThenBy(c => c.Title)
                    .Skip((vm.Page - 1) * vm.PageSize)
                    .Take(vm.PageSize)
                    .ToList();

                vm.Items = _mapper.Map<List<ComponentListItemVM>>(pageItems);

                foreach (var item in vm.Items)
                {
                    var src = pageItems.First(x => x.Id == item.Id);
                    item.ComponentTypeName = typeLookup.TryGetValue(src.ComponentTypeId, out var typeName) ? typeName : "(n/a)";
                }

                return View(vm);
            }

            vm.TotalCount = _componentService.Count(q, onlyActive: false);

            var results = _componentService.Search(q, vm.Page, vm.PageSize, onlyActive: false)
                .OrderBy(c => c.SortOrder ?? int.MaxValue)
                .ThenBy(c => c.Title)
                .ToList();

            vm.Items = _mapper.Map<List<ComponentListItemVM>>(results);

            foreach (var item in vm.Items)
            {
                var src = results.First(x => x.Id == item.Id);
                item.ComponentTypeName = typeLookup.TryGetValue(src.ComponentTypeId, out var typeName) ? typeName : "(n/a)";
            }

            return View(vm);
        }

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
                    .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name })
                    .ToList()
            };

            return View(vm);
        }

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
                }).ToList();

            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                vm.Name = vm.Name.Trim();
                vm.Title = vm.Title.Trim();
                vm.Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim();

                var entity = _mapper.Map<Component>(vm);
                entity.CreatedAt = DateTime.UtcNow;

                // 1) kreiraj komponentu
                _componentService.Create(entity);

                // 2) ako ima upload, spremi sliku i poveži na komponentu
                if (vm.UploadImage != null)
                {
                    var newImageId = SaveImageAndCreateRecord(vm.UploadImage);
                    if (newImageId.HasValue)
                        _componentService.SetImage(entity.Id, newImageId.Value);
                }

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

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var entity = _componentService.GetById(id);
            if (entity == null)
                return NotFound();

            var types = _componentTypeService.GetAll().ToList();

            var vm = _mapper.Map<ComponentEditVM>(entity);
            vm.ComponentTypes = types
                .OrderBy(t => t.DisplayOrder ?? int.MaxValue)
                .ThenBy(t => t.Name)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name,
                    Selected = t.Id == entity.ComponentTypeId
                }).ToList();

            // prikaz postojeće slike
            if (entity.ImageId.HasValue)
            {
                var img = _imageService.GetById(entity.ImageId.Value);
                vm.CurrentImageUrl = img?.StoragePathOrUrl;
            }

            return View(vm);
        }

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
                }).ToList();

            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var existing = _componentService.GetById(vm.Id);
                if (existing == null)
                    return NotFound();

                vm.Name = vm.Name.Trim();
                vm.Title = vm.Title.Trim();
                vm.Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim();

                _mapper.Map(vm, existing);

                _componentService.Update(existing);

                if (vm.UploadImage != null)
                {
                    var newImageId = SaveImageAndCreateRecord(vm.UploadImage);
                    if (newImageId.HasValue)
                        _componentService.SetImage(existing.Id, newImageId.Value);
                }

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
            var entity = _componentService.GetById(id);
            if (entity == null)
                return NotFound();

            var type = _componentTypeService.GetById(entity.ComponentTypeId);

            var vm = _mapper.Map<ComponentDeleteVM>(entity);
            vm.ComponentTypeName = type?.Name ?? "(n/a)";

            return View(vm);
        }

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

        private int? SaveImageAndCreateRecord(IFormFile file)
        {
            if (file == null || file.Length == 0) return null;

            var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowed.Contains(file.ContentType))
                throw new InvalidOperationException("Only JPG, PNG or WEBP images are allowed.");

            // folder wwwroot/uploads
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsDir);

            var ext = Path.GetExtension(file.FileName);
            var storedFileName = $"{Guid.NewGuid():N}{ext}";
            var storedFullPath = Path.Combine(uploadsDir, storedFileName);

            using (var stream = new FileStream(storedFullPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            var url = $"/uploads/{storedFileName}";

            var image = new DAL.Models.Image
            {
                FileName = Path.GetFileName(file.FileName),
                ContentType = file.ContentType,
                Length = file.Length,
                StoragePathOrUrl = url,
                UploadedAt = DateTime.UtcNow
            };

            var imageId = _imageService.Create(image);
            return imageId;
        }
    }
}
