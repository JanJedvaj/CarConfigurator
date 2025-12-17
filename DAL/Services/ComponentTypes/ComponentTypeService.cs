using DAL.Models;
using DAL.Repositories.ComponentTypes;
using System;
using System.Collections.Generic;

namespace DAL.Services.ComponentTypes
{
    public class ComponentTypeService : IComponentTypeService
    {
        private readonly IComponentTypeRepository _componentTypeRepository;

        public ComponentTypeService(IComponentTypeRepository componentTypeRepository)
        {
            _componentTypeRepository = componentTypeRepository;
        }


        public IEnumerable<ComponentType> GetAll()
        {
            return _componentTypeRepository.GetAll();
        }

        public ComponentType? GetById(int id)
        {
            return _componentTypeRepository.GetById(id);
        }

        public int Add(ComponentType type)
        {
            ValidateComponentType(type);

            if (_componentTypeRepository.ExistsByName(type.Name))
                throw new InvalidOperationException("ComponentType name already exists.");

            _componentTypeRepository.Add(type);
            return type.Id;
        }

        public void Update(ComponentType type)
        {
            ValidateComponentType(type);

            var existing = _componentTypeRepository.GetById(type.Id)
                ?? throw new InvalidOperationException("ComponentType not found.");

            if (!string.Equals(existing.Name, type.Name, StringComparison.OrdinalIgnoreCase))
            {
                if (_componentTypeRepository.ExistsByName(type.Name))
                    throw new InvalidOperationException("ComponentType name already exists.");
            }

            _componentTypeRepository.Update(type);
        }

        public void Delete(int id)
        {
            _componentTypeRepository.Delete(id);
        }


        public IEnumerable<ComponentType> GetForConfigurator()
        {
            return _componentTypeRepository.GetForConfigurator();
        }

        //Search + paging

        public IEnumerable<ComponentType> Search(string? query, int page, int pageSize)
        {
            return _componentTypeRepository.Search(query, page, pageSize);
        }

        public int Count(string? query)
        {
            return _componentTypeRepository.Count(query);
        }


        private static void ValidateComponentType(ComponentType type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (string.IsNullOrWhiteSpace(type.Name))
                throw new InvalidOperationException("Name is required.");

            if (type.MinSelect < 0)
                throw new InvalidOperationException("MinSelect must be >= 0.");

            if (type.MaxSelect < type.MinSelect)
                throw new InvalidOperationException("MaxSelect must be >= MinSelect.");
        }
    }
}
