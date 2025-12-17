using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DAL.Repositories.Components
{
    public class ComponentRepository : IComponentRepository
    {
        private readonly CarConfiguratorDbContext _context;

        public ComponentRepository(CarConfiguratorDbContext context)
        {
            _context = context;
        }


        public IEnumerable<Component> GetAll()
        {
            return _context.Components
                .OrderBy(c => c.SortOrder ?? int.MaxValue)
                .ThenBy(c => c.Title)
                .ToList();
        }

        public IEnumerable<Component> GetAllWithType()
        {
            return _context.Components
                .Include(c => c.ComponentType)
                .Include(c => c.Image)
                .OrderBy(c => c.SortOrder ?? int.MaxValue)
                .ThenBy(c => c.Title)
                .ToList();
        }

        public Component? GetById(int id)
        {
            return _context.Components
                .Include(c => c.ComponentType)
                .Include(c => c.Image)
                .FirstOrDefault(c => c.Id == id);
        }

        public void Add(Component component)
        {
            _context.Components.Add(component);
            _context.SaveChanges();
        }

        public void Update(Component component)
        {
            _context.Components.Update(component);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var component = _context.Components
                .Include(c => c.CarConfigurationComponents)
                .Include(c => c.ComponentCompatibilityComponents)
                .Include(c => c.ComponentCompatibilityCompatibleWithComponents)
                .FirstOrDefault(c => c.Id == id);

            if (component == null)
                return;

            // ukloni stavke konfiguracija
            if (component.CarConfigurationComponents.Any())
                _context.CarConfigurationComponents
                    .RemoveRange(component.CarConfigurationComponents);

            // ukloni kompatibilnosti (gdje je component)
            if (component.ComponentCompatibilityComponents.Any())
                _context.ComponentCompatibilities
                    .RemoveRange(component.ComponentCompatibilityComponents);

            // ukloni kompatibilnosti (gdje je compatibleWithComponent)
            if (component.ComponentCompatibilityCompatibleWithComponents.Any())
                _context.ComponentCompatibilities
                    .RemoveRange(component.ComponentCompatibilityCompatibleWithComponents);

            _context.Components.Remove(component);
            _context.SaveChanges();
        }


        public IEnumerable<Component> GetByComponentType(int componentTypeId)
        {
            return _context.Components
                .Where(c =>
                    c.ComponentTypeId == componentTypeId &&
                    c.IsActive)
                .Include(c => c.Image)
                .OrderBy(c => c.SortOrder ?? int.MaxValue)
                .ThenBy(c => c.Title)
                .ToList();
        }

        //Search + paging

        public int Count(string? query)
        {
            var q = _context.Components.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                q = q.Where(c =>
                    c.Name.Contains(query) ||
                    c.Title.Contains(query));
            }

            return q.Count();
        }

        public IEnumerable<Component> Search(string? query, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var q = _context.Components
                .Include(c => c.ComponentType)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                q = q.Where(c =>
                    c.Name.Contains(query) ||
                    c.Title.Contains(query));
            }

            return q
                .OrderBy(c => c.SortOrder ?? int.MaxValue)
                .ThenBy(c => c.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
    }
}
