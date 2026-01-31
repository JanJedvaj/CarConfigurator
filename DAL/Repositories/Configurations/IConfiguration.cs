using DAL.Models;
using System.Collections.Generic;

namespace DAL.Repositories.Configurations
{
    public interface IConfigurationRepository
    {
        //Configuration CRUD
        IEnumerable<CarConfiguration> GetByUser(int userId);
        CarConfiguration? GetById(int id);
        CarConfiguration? GetDetails(int id);

        int Add(CarConfiguration configuration);
        void Update(CarConfiguration configuration);
        void Delete(int id);

        //Configuration items M-N
        void AddComponent(int configurationId, int componentId);
        void RemoveComponent(int configurationId, int componentId);
        void ClearComponents(int configurationId);

        //Biznis helpers
        decimal RecalculateTotalPrice(int configurationId);
    }
}
