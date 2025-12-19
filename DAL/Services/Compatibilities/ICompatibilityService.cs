using DAL.Models;
using System.Collections.Generic;

namespace DAL.Services.Compatibilities
{
    public interface ICompatibilityService
    {
        IEnumerable<ComponentCompatibility> GetAll();
        void SetRule(int componentId, int compatibleWithComponentId, bool isAllowed);
        void DeleteRule(int componentId, int compatibleWithComponentId);
        void SetAllowedForComponent(int componentId, IEnumerable<int> allowedCompatibleIds);
        IEnumerable<int> GetAllowedCompatibleComponentIds(int componentId);
        bool IsAllowed(int componentId, int compatibleWithComponentId);
        void DeleteAllForComponent(int componentId);
    }
}
