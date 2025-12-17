using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DAL.Repositories.Configurations
{
    public class ConfigurationRepository : IConfigurationRepository
    {
        private readonly CarConfiguratorDbContext _context;

        public ConfigurationRepository(CarConfiguratorDbContext context)
        {
            _context = context;
        }

        //CRUD

        public IEnumerable<CarConfiguration> GetByUser(int userId)
        {
            return _context.CarConfigurations
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();
        }

        public CarConfiguration? GetById(int id)
        {
            return _context.CarConfigurations.Find(id);
        }

        public CarConfiguration? GetDetails(int id)
        {
            return _context.CarConfigurations
                .Include(c => c.CarConfigurationComponents)
                    .ThenInclude(cc => cc.Component)
                .FirstOrDefault(c => c.Id == id);
        }

        public int Add(CarConfiguration configuration)
        {
            if (configuration.CreatedAt == default)
                configuration.CreatedAt = DateTime.UtcNow;

            _context.CarConfigurations.Add(configuration);
            _context.SaveChanges();
            return configuration.Id;
        }

        public void Update(CarConfiguration configuration)
        {
            configuration.UpdatedAt = DateTime.UtcNow;
            _context.CarConfigurations.Update(configuration);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var cfg = _context.CarConfigurations
                .Include(c => c.CarConfigurationComponents)
                .FirstOrDefault(c => c.Id == id);

            if (cfg == null)
                return;

            // prvo makni M-N zapise
            if (cfg.CarConfigurationComponents.Any())
                _context.CarConfigurationComponents.RemoveRange(cfg.CarConfigurationComponents);

            _context.CarConfigurations.Remove(cfg);
            _context.SaveChanges();
        }

        //Configuration items (M-N)

        public void AddComponent(int configurationId, int componentId)
        {
            var exists = _context.CarConfigurationComponents.Any(x =>
                x.CarConfigurationId == configurationId &&
                x.ComponentId == componentId);

            if (exists)
                return;

            _context.CarConfigurationComponents.Add(new CarConfigurationComponent
            {
                CarConfigurationId = configurationId,
                ComponentId = componentId,
                AddedAt = DateTime.UtcNow
            });

            _context.SaveChanges();
        }

        public void RemoveComponent(int configurationId, int componentId)
        {
            var item = _context.CarConfigurationComponents.FirstOrDefault(x =>
                x.CarConfigurationId == configurationId &&
                x.ComponentId == componentId);

            if (item == null)
                return;

            _context.CarConfigurationComponents.Remove(item);
            _context.SaveChanges();
        }

        public void ClearComponents(int configurationId)
        {
            var items = _context.CarConfigurationComponents
                .Where(x => x.CarConfigurationId == configurationId)
                .ToList();

            if (!items.Any())
                return;

            _context.CarConfigurationComponents.RemoveRange(items);
            _context.SaveChanges();
        }

        //Business helpers

        public decimal RecalculateTotalPrice(int configurationId)
        {
            var total = _context.CarConfigurationComponents
                .Where(x => x.CarConfigurationId == configurationId)
                .Join(
                    _context.Components,
                    cc => cc.ComponentId,
                    c => c.Id,
                    (cc, c) => c.Price
                )
                .Sum();

            var cfg = _context.CarConfigurations.Find(configurationId)
                ?? throw new InvalidOperationException("Configuration not found.");

            cfg.TotalPrice = total;
            cfg.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();
            return total;
        }
    }
}
