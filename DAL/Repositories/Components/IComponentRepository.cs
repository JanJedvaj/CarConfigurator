using DAL.Models;
using System.Collections.Generic;

namespace DAL.Repositories.Components
{
    public interface IComponentRepository
    {
        IEnumerable<Component> GetAll();
        IEnumerable<Component> GetAllWithType();
        Component? GetById(int id);
        void Add(Component component);
        void Update(Component component);
        void Delete(int id);

        // Configurator
        IEnumerable<Component> GetByComponentType(int componentTypeId);

        //search + paging
        IEnumerable<Component> Search(string? query, int page, int pageSize);
        int Count(string? query);
    }
}
