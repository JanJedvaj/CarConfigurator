using DAL.Models;
using DAL.Repositories.Compatibilities;
using DAL.Repositories.Components;
using System;
using System.Collections.Generic;

namespace DAL.Services.Compatibilities
{
    public class CompatibilityService : ICompatibilityService
    {
        private readonly ICompatibilityRepository _compatibilityRepository;
        private readonly IComponentRepository _componentRepository;

        public CompatibilityService(
            ICompatibilityRepository compatibilityRepository,
            IComponentRepository componentRepository)
        {
            _compatibilityRepository = compatibilityRepository;
            _componentRepository = componentRepository;
        }


        public IEnumerable<ComponentCompatibility> GetAll()
            => _compatibilityRepository.GetAll();

        public void SetRule(int componentId, int compatibleWithComponentId, bool isAllowed)
        {
            if (componentId <= 0 || compatibleWithComponentId <= 0)
                throw new InvalidOperationException("ComponentId and CompatibleWithComponentId are required.");

            if (componentId == compatibleWithComponentId)
                throw new InvalidOperationException("Component cannot be compatible with itself.");

            var a = _componentRepository.GetById(componentId)
                ?? throw new InvalidOperationException("Component not found.");

            var b = _componentRepository.GetById(compatibleWithComponentId)
                ?? throw new InvalidOperationException("CompatibleWith component not found.");
            var existing = _compatibilityRepository.GetByIds(componentId, compatibleWithComponentId);
            if (existing == null)
            {
                _compatibilityRepository.Add(new ComponentCompatibility
                {
                    ComponentId = componentId,
                    CompatibleWithComponentId = compatibleWithComponentId,
                    IsAllowed = isAllowed,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                existing.IsAllowed = isAllowed;
                _compatibilityRepository.Update(existing);
            }
        }

        public void DeleteRule(int componentId, int compatibleWithComponentId)
        {
            _compatibilityRepository.Delete(componentId, compatibleWithComponentId);
        }

        public void SetAllowedForComponent(int componentId, IEnumerable<int> allowedCompatibleIds)
        {
            if (componentId <= 0)
                throw new InvalidOperationException("ComponentId is required.");
            var comp = _componentRepository.GetById(componentId)
                ?? throw new InvalidOperationException("Component not found.");

            _compatibilityRepository.SetAllowedForComponent(componentId, allowedCompatibleIds);
        }

        public IEnumerable<int> GetAllowedCompatibleComponentIds(int componentId)
        {
            if (componentId <= 0)
                throw new InvalidOperationException("ComponentId is required.");

            return _compatibilityRepository.GetAllowedCompatibleComponentIds(componentId);
        }

        public bool IsAllowed(int componentId, int compatibleWithComponentId)
            => _compatibilityRepository.IsAllowed(componentId, compatibleWithComponentId);

        public void DeleteAllForComponent(int componentId)
            => _compatibilityRepository.DeleteAllForComponent(componentId);
    }
}
