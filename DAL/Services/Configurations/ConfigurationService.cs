using DAL.Models;
using DAL.Repositories.ComponentTypes;
using DAL.Repositories.Components;
using DAL.Repositories.Configurations;
using DAL.Repositories.Compatibilities;
using System;
using System.Collections.Generic;
using System.Linq;
using DAL.Services.Configurations;

namespace DAL.Services.Configurations
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IComponentRepository _componentRepository;
        private readonly IComponentTypeRepository _componentTypeRepository;
        private readonly ICompatibilityRepository _compatibilityRepository;

        public ConfigurationService(
            IConfigurationRepository configurationRepository,
            IComponentRepository componentRepository,
            IComponentTypeRepository componentTypeRepository,
            ICompatibilityRepository compatibilityRepository)
        {
            _configurationRepository = configurationRepository;
            _componentRepository = componentRepository;
            _componentTypeRepository = componentTypeRepository;
            _compatibilityRepository = compatibilityRepository;
        }


        public IEnumerable<CarConfiguration> GetUserConfigurations(int userId)
            => _configurationRepository.GetByUser(userId);

        public CarConfiguration? GetConfiguration(int id)
            => _configurationRepository.GetById(id);

        public CarConfiguration? GetConfigurationDetails(int id)
            => _configurationRepository.GetDetails(id);

        public int CreateConfiguration(int userId, string name)
        {
            if (userId <= 0) throw new InvalidOperationException("UserId is required.");
            if (string.IsNullOrWhiteSpace(name)) throw new InvalidOperationException("Name is required.");

            var cfg = new CarConfiguration
            {
                UserId = userId,
                Name = name,
                CreatedAt = DateTime.UtcNow,
                TotalPrice = 0
            };

            return _configurationRepository.Add(cfg);
        }

        public void UpdateConfiguration(CarConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (string.IsNullOrWhiteSpace(configuration.Name))
                throw new InvalidOperationException("Name is required.");

            _configurationRepository.Update(configuration);
        }

        public void DeleteConfiguration(int id)
            => _configurationRepository.Delete(id);

        public void AddComponentToConfiguration(int configurationId, int componentId)
        {

            var cfg = _configurationRepository.GetDetails(configurationId)
                ?? throw new InvalidOperationException("Configuration not found.");

     
            var component = _componentRepository.GetById(componentId)
                ?? throw new InvalidOperationException("Component not found.");

            if (!component.IsActive)
                throw new InvalidOperationException("Component is not active.");


            var type = _componentTypeRepository.GetById(component.ComponentTypeId)
                ?? throw new InvalidOperationException("ComponentType not found.");


            var selectedComponentIds = cfg.CarConfigurationComponents.Select(x => x.ComponentId).ToList();

            var selectedSameTypeCount = selectedComponentIds
                .Select(id => _componentRepository.GetById(id))
                .Where(c => c != null && c.ComponentTypeId == component.ComponentTypeId)
                .Count();

            if (type.MaxSelect >= 0 && selectedSameTypeCount >= type.MaxSelect)
                throw new InvalidOperationException($"You can select max {type.MaxSelect} item(s) for type '{type.Name}'.");

            foreach (var selectedId in selectedComponentIds)
            {
                if (selectedId == componentId) continue;

                var allowedA = _compatibilityRepository.IsAllowed(selectedId, componentId);
                var allowedB = _compatibilityRepository.IsAllowed(componentId, selectedId);

                 if (!allowedA && !allowedB)
                    throw new InvalidOperationException("Selected component is not compatible with current configuration.");
            }

            _configurationRepository.AddComponent(configurationId, componentId);


            _configurationRepository.RecalculateTotalPrice(configurationId);
        }

        public void RemoveComponentFromConfiguration(int configurationId, int componentId)
        {
            _configurationRepository.RemoveComponent(configurationId, componentId);
            _configurationRepository.RecalculateTotalPrice(configurationId);
        }

        public void ClearConfiguration(int configurationId)
        {
            _configurationRepository.ClearComponents(configurationId);
            _configurationRepository.RecalculateTotalPrice(configurationId);
        }


        public decimal RecalculateTotalPrice(int configurationId)
            => _configurationRepository.RecalculateTotalPrice(configurationId);
    }
}
