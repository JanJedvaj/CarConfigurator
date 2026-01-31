using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DAL.Repositories.Compatibilities
{
    public class CompatibilityRepository : ICompatibilityRepository
    {
        private readonly CarConfiguratorDbContext _context;

        public CompatibilityRepository(CarConfiguratorDbContext context)
        {
            _context = context;
        }

        //CRUD-ish 

        public IEnumerable<ComponentCompatibility> GetAll()
        {
            return _context.ComponentCompatibilities
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }

        public ComponentCompatibility? GetByIds(int componentId, int compatibleWithComponentId)
        {
            return _context.ComponentCompatibilities
                .FirstOrDefault(x =>
                    x.ComponentId == componentId &&
                    x.CompatibleWithComponentId == compatibleWithComponentId);
        }

        public void Add(ComponentCompatibility rule)
        {
            // zaštita od self-kompatibilnosti (u bazi ima CHECK, ali bolje i ovdje)
            if (rule.ComponentId == rule.CompatibleWithComponentId)
                throw new InvalidOperationException("Component cannot be compatible with itself.");

            // spriječi duplikat (PK je composite pa bi ionako puklo)
            var exists = GetByIds(rule.ComponentId, rule.CompatibleWithComponentId);
            if (exists != null)
                throw new InvalidOperationException("Compatibility rule already exists.");

            // default CreatedAt ako nije postavljen
            if (rule.CreatedAt == default)
                rule.CreatedAt = DateTime.UtcNow;

            _context.ComponentCompatibilities.Add(rule);
            _context.SaveChanges();
        }

        public void Update(ComponentCompatibility rule)
        {
            var existing = GetByIds(rule.ComponentId, rule.CompatibleWithComponentId);
            if (existing == null)
                throw new InvalidOperationException("Compatibility rule not found.");

            existing.IsAllowed = rule.IsAllowed;

            _context.SaveChanges();
        }

        public void Delete(int componentId, int compatibleWithComponentId)
        {
            var existing = GetByIds(componentId, compatibleWithComponentId);
            if (existing == null)
                return;

            _context.ComponentCompatibilities.Remove(existing);
            _context.SaveChanges();
        }

        //Biznis metode 

        public bool IsAllowed(int componentId, int compatibleWithComponentId)
        {
            return _context.ComponentCompatibilities.Any(x =>
                x.ComponentId == componentId &&
                x.CompatibleWithComponentId == compatibleWithComponentId &&
                x.IsAllowed);
        }

        public IEnumerable<int> GetAllowedCompatibleComponentIds(int componentId)
        {
            return _context.ComponentCompatibilities
                .Where(x => x.ComponentId == componentId && x.IsAllowed)
                .Select(x => x.CompatibleWithComponentId)
                .ToList();
        }

        public void DeleteAllForComponent(int componentId)
        {
            var rows = _context.ComponentCompatibilities
                .Where(x => x.ComponentId == componentId || x.CompatibleWithComponentId == componentId)
                .ToList();

            if (!rows.Any())
                return;

            _context.ComponentCompatibilities.RemoveRange(rows);
            _context.SaveChanges();
        }

        public void SetAllowedForComponent(int componentId, IEnumerable<int> allowedCompatibleIds)
        {
            var allowedSet = new HashSet<int>(allowedCompatibleIds ?? Enumerable.Empty<int>());

            if (allowedSet.Contains(componentId))
                allowedSet.Remove(componentId);

            // postojeća pravila za tu komponentu
            var existing = _context.ComponentCompatibilities
                .Where(x => x.ComponentId == componentId)
                .ToList();

            // 1) update postojeće: postavi IsAllowed ovisno je li u listi
            foreach (var row in existing)
            {
                row.IsAllowed = allowedSet.Contains(row.CompatibleWithComponentId);
            }

            // 2) dodaj nova pravila koja ne postoje još
            var existingIds = existing.Select(x => x.CompatibleWithComponentId).ToHashSet();

            foreach (var id in allowedSet)
            {
                if (!existingIds.Contains(id))
                {
                    _context.ComponentCompatibilities.Add(new ComponentCompatibility
                    {
                        ComponentId = componentId,
                        CompatibleWithComponentId = id,
                        IsAllowed = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            _context.SaveChanges();
        }
    }
}
