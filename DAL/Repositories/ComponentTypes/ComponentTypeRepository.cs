using DAL.Models;
using System.Collections.Generic;
using System.Linq;

namespace DAL.Repositories.ComponentTypes
{
    public class ComponentTypeRepository : IComponentTypeRepository
    {
        private readonly CarConfiguratorDbContext _context;

        public ComponentTypeRepository(CarConfiguratorDbContext context)
        {
            _context = context;
        }
        public IEnumerable<ComponentType> GetAll()
        {
            return _context.ComponentTypes
                .OrderBy(x => x.DisplayOrder ?? int.MaxValue)
                .ThenBy(x => x.Name)
                .ToList();
        }

        public ComponentType? GetById(int id)
        {
            return _context.ComponentTypes.Find(id);
        }

        public ComponentType? GetByName(string name)
        {
            return _context.ComponentTypes.FirstOrDefault(x => x.Name == name);
        }

        public void Add(ComponentType type)
        {
            _context.ComponentTypes.Add(type);
            _context.SaveChanges();
        }

        public void Update(ComponentType type)
        {
            _context.ComponentTypes.Update(type);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var type = _context.ComponentTypes.Find(id);
            if (type == null)
                return;

            //ne daj brisanje ako postoje komponente tog tipa
            var hasComponents = _context.Components.Any(c => c.ComponentTypeId == id);
            if (hasComponents)
                throw new System.InvalidOperationException("Cannot delete ComponentType because there are Components using it.");

            _context.ComponentTypes.Remove(type);
            _context.SaveChanges();
        }

        //Configurator

        public IEnumerable<ComponentType> GetForConfigurator()
        {
            return _context.ComponentTypes
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder ?? int.MaxValue)
                .ThenBy(x => x.Name)
                .ToList();
        }

        //Search + paging 
        public int Count(string? query)
        {
            var q = _context.ComponentTypes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
                q = q.Where(x => x.Name.Contains(query));

            return q.Count();
        }

        public IEnumerable<ComponentType> Search(string? query, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var q = _context.ComponentTypes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
                q = q.Where(x => x.Name.Contains(query));

            return q
                .OrderBy(x => x.DisplayOrder ?? int.MaxValue)
                .ThenBy(x => x.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }


        public bool ExistsByName(string name)
        {
            return _context.ComponentTypes.Any(x => x.Name == name);
        }
    }
}
