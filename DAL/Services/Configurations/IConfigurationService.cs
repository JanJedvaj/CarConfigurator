using DAL.Models;
using System.Collections.Generic;

namespace DAL.Services.Configurations
{
    public interface IConfigurationService
    {
        IEnumerable<CarConfiguration> GetUserConfigurations(int userId);
        CarConfiguration? GetConfiguration(int id);
        CarConfiguration? GetConfigurationDetails(int id);

        int CreateConfiguration(int userId, string name);
        void UpdateConfiguration(CarConfiguration configuration);
        void DeleteConfiguration(int id);

        void AddComponentToConfiguration(int configurationId, int componentId);
        void RemoveComponentFromConfiguration(int configurationId, int componentId);
        void ClearConfiguration(int configurationId);

        decimal RecalculateTotalPrice(int configurationId);
    }
}
