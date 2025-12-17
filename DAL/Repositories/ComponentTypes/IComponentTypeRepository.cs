using DAL.Models;
using System.Collections.Generic;

namespace DAL.Repositories.ComponentTypes
{
    public interface IComponentTypeRepository
    {
        IEnumerable<ComponentType> GetAll();
        ComponentType? GetById(int id);
        void Add(ComponentType type);
        void Update(ComponentType type);
        void Delete(int id);

        // Za konfigurator (aktivni + sortirani)
        IEnumerable<ComponentType> GetForConfigurator();

        // Poželjno: search + paging
        IEnumerable<ComponentType> Search(string? query, int page, int pageSize);
        int Count(string? query);

        // Validacija unique name
        bool ExistsByName(string name);
        ComponentType? GetByName(string name);
    }
}
