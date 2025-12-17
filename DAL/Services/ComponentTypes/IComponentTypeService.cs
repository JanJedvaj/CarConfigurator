using DAL.Models;
using System.Collections.Generic;

namespace DAL.Services.ComponentTypes
{
    public interface IComponentTypeService
    {
        IEnumerable<ComponentType> GetAll();
        ComponentType? GetById(int id);
        int Add(ComponentType type);
        void Update(ComponentType type);
        void Delete(int id);

        // Korisnički dio / konfigurator
        IEnumerable<ComponentType> GetForConfigurator();

        // Search + paging 
        IEnumerable<ComponentType> Search(string? query, int page, int pageSize);
        int Count(string? query);
    }
}
