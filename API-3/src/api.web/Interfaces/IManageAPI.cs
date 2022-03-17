using api.web.Entities;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace api.web.Implementations
{
    public interface IManageAPI
    {
        List<List<Node>> TCApiData { get; set; }

        Task SaveInput(IEnumerable<VehicleRecallModel> vrData, string fileName);
        Task<bool> CallVRDApiAsync(IEnumerable<VehicleRecallModel> vrData);
        Task AddContentToJsonSource(IEnumerable<VehicleRecallModel> vrData, string fileName);
        Task<IEnumerable<VehicleRecallModel>> GetAllData();
        Task<IEnumerable<VehicleRecallModel>> GetBySystemType(string systemType);
        Task<VehicleRecallModel> GetByRecallNumber(string recallNumber);
    }
}