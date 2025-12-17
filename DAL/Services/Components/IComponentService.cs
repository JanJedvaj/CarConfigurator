using DAL.Models;
using System.Collections.Generic;

namespace DAL.Services.Components
{
    public interface IComponentService
    {
        IEnumerable<Component> GetAll(bool onlyActive = false);
        Component? GetById(int id);
        void Create(Component component);
        void Update(Component component);
        void Delete(int id);

        // Configurator helpers
        IEnumerable<Component> GetByComponentType(int componentTypeId);

        // Poželjno: search + paging
        IEnumerable<Component> Search(string? query, int page, int pageSize, bool onlyActive = false);
        int Count(string? query, bool onlyActive = false);

        // Povezivanje slike (admin)
        void SetImage(int componentId, int? imageId);

        //toggle active
        void SetActive(int componentId, bool isActive);
    }
}
