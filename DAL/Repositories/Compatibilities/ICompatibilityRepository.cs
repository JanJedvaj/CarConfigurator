using DAL.Models;
using System.Collections.Generic;

namespace DAL.Repositories.Compatibilities
{
    public interface ICompatibilityRepository
    {
        IEnumerable<ComponentCompatibility> GetAll();
        ComponentCompatibility? GetByIds(int componentId, int compatibleWithComponentId);

        void Add(ComponentCompatibility rule);       // dodaj novo pravilo
        void Update(ComponentCompatibility rule);    // promijeni IsAllowed
        void Delete(int componentId, int compatibleWithComponentId);

        // Business metode za konfigurator
        bool IsAllowed(int componentId, int compatibleWithComponentId);
        IEnumerable<int> GetAllowedCompatibleComponentIds(int componentId);

        // Korisno kod brisanja komponente (zbog NO ACTION na jednom FK-u)
        void DeleteAllForComponent(int componentId);

        // Admin helper: masovno postavljanje kompatibilnosti za jednu komponentu
        // (npr. admin označi 10 kompatibilnih od 50)
        void SetAllowedForComponent(int componentId, IEnumerable<int> allowedCompatibleIds);
    }
}
