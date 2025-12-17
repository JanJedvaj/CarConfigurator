using DAL.Models;
using DAL.Repositories.Components;
using DAL.Repositories.Compatibilities;
using DAL.Repositories.Images;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DAL.Services.Components
{
    public class ComponentService : IComponentService
    {
        private readonly IComponentRepository _componentRepository;
        private readonly ICompatibilityRepository _compatibilityRepository;
        private readonly IImageRepository _imageRepository;

        public ComponentService(
            IComponentRepository componentRepository,
            ICompatibilityRepository compatibilityRepository,
            IImageRepository imageRepository)
        {
            _componentRepository = componentRepository;
            _compatibilityRepository = compatibilityRepository;
            _imageRepository = imageRepository;
        }

        public IEnumerable<Component> GetAll(bool onlyActive = false)
        {
            var items = _componentRepository.GetAllWithType();

            if (onlyActive)
                items = items.Where(x => x.IsActive);

            return items;
        }

        public Component? GetById(int id)
        {
            return _componentRepository.GetById(id);
        }

        public void Create(Component component)
        {
            Validate(component);

            _componentRepository.Add(component);
        }

        public void Update(Component component)
        {
            Validate(component);

            var existing = _componentRepository.GetById(component.Id)
                ?? throw new InvalidOperationException("Component not found.");


            _componentRepository.Update(component);
        }

        public void Delete(int id)
        {
            _compatibilityRepository.DeleteAllForComponent(id);
            _componentRepository.Delete(id);
        }


        public IEnumerable<Component> GetByComponentType(int componentTypeId)
        {
            return _componentRepository.GetByComponentType(componentTypeId);
        }

        //Search + paging 

        public IEnumerable<Component> Search(string? query, int page, int pageSize, bool onlyActive = false)
        {
            var result = _componentRepository.Search(query, page, pageSize);

            if (onlyActive)
                result = result.Where(x => x.IsActive);

            return result;
        }

        public int Count(string? query, bool onlyActive = false)
        {
            if (!onlyActive)
                return _componentRepository.Count(query);

            return _componentRepository.GetAll().Count(x =>
                (string.IsNullOrWhiteSpace(query) || x.Name.Contains(query) || x.Title.Contains(query)) &&
                x.IsActive);
        }

        //Image linking

        public void SetImage(int componentId, int? imageId)
        {
            var component = _componentRepository.GetById(componentId)
                ?? throw new InvalidOperationException("Component not found.");

            if (imageId.HasValue)
            {
                var img = _imageRepository.GetById(imageId.Value);
                if (img == null)
                    throw new InvalidOperationException("Image not found.");
            }

            component.ImageId = imageId;
            _componentRepository.Update(component);
        }

        //Active flag

        public void SetActive(int componentId, bool isActive)
        {
            var component = _componentRepository.GetById(componentId)
                ?? throw new InvalidOperationException("Component not found.");

            component.IsActive = isActive;
            _componentRepository.Update(component);
        }


        private static void Validate(Component component)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));

            if (string.IsNullOrWhiteSpace(component.Name))
                throw new InvalidOperationException("Name is required.");

            if (string.IsNullOrWhiteSpace(component.Title))
                throw new InvalidOperationException("Title is required.");

            if (component.Price < 0)
                throw new InvalidOperationException("Price must be >= 0.");

            if (component.ComponentTypeId <= 0)
                throw new InvalidOperationException("ComponentTypeId is required.");
        }
    }
}
